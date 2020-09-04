using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

public class Movement : MonoBehaviour
{
    private float throttle, throttleMax, rpm, ftit, fuel, rpmMax, ftitMax, fuelMax, rot_turn, shakeAmount = 5f, rpmtime, explosiontime,random;
    public Image standbygen, maingen, enginewarning, jfsrun, seccaution, dummy;
    public Transform throttle_needle,rpm_needle,ftit_needle,fuel_needle, camTransform, planeTransform, adi_transform;
    private const float max_angle_throttle = 0, max_angle_rpm= -191.992f, max_angle_ftit= -127f, max_angle_fuel= 279.267f;
    private const float zero_angle_throttle = -104, zero_angle_rpm= 133.233f, zero_angle_ftit= 85f, zero_angle_fuel= -17.781f;
    private Color defaultColor;
    private string[] words;
    public Text throttle_text, ftit_text, fuel_text, rpm_text, altitude_text, enginestatus_text, enginestatus_text_2, enginestatus_text_3;
    Vector3 originalCamPos,originalPlanePos;
    float start;
    public Rigidbody rb;
    public ParticleSystem explosion, fire;
    bool isExpPlayed, isCrashed;
    private float GetThrottleRotation()//returns specific angle for the Throttle
    {
        float totalAngleSize = zero_angle_throttle - max_angle_throttle;
        float throttleNormalized = throttle / throttleMax;
        return zero_angle_throttle + throttleNormalized * totalAngleSize * 2.45f;
    }
    private float GetFuelRotation()//returns specific angle for the Fuel
    {
        float totalAngleSize = zero_angle_fuel - max_angle_fuel;
        float fuelNormalized = fuel/ fuelMax;
        return zero_angle_fuel - fuelNormalized * totalAngleSize;
    }
    private float GetRPMRotation()//returns specific angle for the RPM
    {
        float totalAngleSize = zero_angle_rpm - max_angle_rpm;
        float rpmNormalized = rpm/rpmMax;
        return zero_angle_rpm - rpmNormalized * totalAngleSize;
    }
    private float GetFTITRotation()//returns specific angle for the FTIT
    {
        float totalAngleSize = zero_angle_ftit - max_angle_ftit;
        float ftitNormalized = ftit/ ftitMax;
        return zero_angle_ftit - ftitNormalized * totalAngleSize;
    }
    private string GetData(string throttle)//returns RPM,FTIT and FUEL data according the Throttle value
    {
        string csvFile = "Assets/throttle_valuecsv.csv";
        foreach (string line in File.ReadLines(csvFile))
            foreach (string value in line.Replace("\"", "").Split('\r', '\n', ','))
                if (value.Trim() == throttle.Trim())
                    return "["+ value + "] : "+ line;
        return "";
    }
    void OnEnable()
    {
        originalCamPos = camTransform.localPosition;
        originalPlanePos = planeTransform.localPosition;
    }
    void Start()
    {
        start = 0;
        throttle = 0f;
        throttleMax = 127f;
        rpmMax = 100.88f;
        ftitMax = 782.28f;
        fuelMax = 40958.68f;
        defaultColor = dummy.color;
        rpmtime = 0;
        explosiontime = 0;
        isExpPlayed = false;
        isCrashed = false;
        fire.Stop();
        explosion.Stop();
    }
    void Update()
    {
        
        altitude_text.text = Math.Floor(transform.position.y).ToString() + " meters";
        if (isCrashed)
        {
            fire.Play();
            rb.detectCollisions = true;
            rb.isKinematic = false;
        }
        if (Input.GetKeyDown(KeyCode.F) && !isCrashed)
        {
            random = UnityEngine.Random.Range(1, 10);
            start++;
        }
        if (start % 2 == 0 && !isCrashed)
        {
            rb.detectCollisions = true;
            rb.isKinematic = false;
            enginestatus_text.text = "Engine Off";
            enginestatus_text_2.text = "";
            transform.position += transform.forward * Time.deltaTime * throttle;
            rpmtime = 0;
        }
        if(start%2==1 && random <= 3 && !isCrashed)
        {
            enginestatus_text.text = "No Start";
            enginestatus_text_2.text = "Restart the Engine";
            transform.position += transform.forward * Time.deltaTime * throttle;
            rpmtime = 0;
        }
        if(start%2==1 && random>=4 &&random<=6 && !isCrashed)
        {
            enginestatus_text.text = "Slow Start";
            enginestatus_text_2.text = "Increase Speed";
            if (throttle >= 30) random = 7;
            Fly(2);
        }
        if (start % 2 == 1 && random > 6 && !isCrashed)
        {
            enginestatus_text.text = "Engine On";
            Fly(10);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag=="Ground")
        {
            start = 0.1f;
            isCrashed = true;
            enginestatus_text.text = "";
            enginestatus_text_2.text = "Crashed";
            enginestatus_text_3.text = "";
            if (!isExpPlayed)
            {
                explosion.Play();
                isExpPlayed = true;
            }
            rb.detectCollisions = false;
            rb.isKinematic = true;
        }
    }
    void Fly(int speed)
    {
        if (throttle <= 18) enginestatus_text_3.text = "IDLE";
        if (throttle >= 19 && throttle<83) enginestatus_text_3.text = "MIL";
        if (throttle >= 83) enginestatus_text_3.text = "AB";
        rb.detectCollisions = false;
        rb.isKinematic = true;
        if (Input.GetKeyDown(KeyCode.Mouse0)) throttle += speed;
        if (Input.GetKeyDown(KeyCode.Mouse1)) throttle -= speed;
        if (throttle > 127) throttle = 127;
        if (throttle < 0) throttle = 0;
        if (throttle >= 0 && throttle <= 127)
        {
            rot_turn = transform.eulerAngles.z;
            adi_transform.localRotation = Quaternion.Euler(adi_transform.localRotation.x, adi_transform.localRotation.y, -rot_turn);
            words = GetData(Math.Floor(throttle).ToString()).Split(',');
            rpm = float.Parse(words[3].ToString().Trim('"')) / 100;
            fuel = float.Parse(words[1].ToString().Trim('"')) / 100;
            ftit = float.Parse(words[2].ToString().Trim('"')) / 100;
            if (rpm < 60)
            {
                standbygen.color = Color.red;
                enginewarning.color = Color.red;
            }
            if (rpm >= 60)
            {
                standbygen.color = defaultColor;
                enginewarning.color = defaultColor;
            }
            if (rpm < 80)
            {
                rpmtime += Time.deltaTime;
                enginestatus_text_2.text = "Increase RPM";
            }
            if (throttle == 0) rpmtime = 0;
            if (rpm > 80 && throttle<=100)
            {
                rpmtime = 0;
                ftit = float.Parse(words[2].ToString().Trim('"')) / 100;
                enginestatus_text_2.text = "Fine";
            }
            if (rpmtime > 5)
            {
                explosiontime += Time.deltaTime;
                enginestatus_text.text = "Hot Start";
                enginestatus_text_2.text = "Increase RPM";
                ftit += 200;
            }
            if (explosiontime > 5)
            {
                enginestatus_text.text = "Hung Start";
                start++;
                explosion.Play();
                fire.Play();
                while(throttle>=0) throttle -= Time.deltaTime;
                transform.position += transform.forward * Time.deltaTime * throttle;
                isCrashed = true;
                rpmtime = 0;
            }
            if (rpm < 60.225) maingen.color = Color.red;
            if (rpm >= 60.225) maingen.color = defaultColor;
            if (rpm < 55) jfsrun.color = Color.red;
            if (rpm >= 55) jfsrun.color = defaultColor;
            if (rpm < 20) seccaution.color = Color.red;
            if (rpm >= 20) seccaution.color = defaultColor;
            throttle_needle.eulerAngles = new Vector3(0, 0, GetThrottleRotation());
            rpm_needle.eulerAngles = new Vector3(0, 0, GetRPMRotation() + 50);//Added 50 because, otherwise it wont fit the indicator
            fuel_needle.eulerAngles = new Vector3(0, 0, GetFuelRotation());
            ftit_needle.eulerAngles = new Vector3(0, 0, GetFTITRotation());
            throttle_text.text = Math.Floor(throttle).ToString();
            rpm_text.text = rpm.ToString();
            fuel_text.text = fuel.ToString();
            ftit_text.text = ftit.ToString();
            throttle += Input.GetAxis("Mouse ScrollWheel");
            transform.position += transform.forward * Time.deltaTime * throttle;
            transform.Rotate(Input.GetAxis("Vertical"), 0.0f, -Input.GetAxis("Horizontal"));
            if (throttle > 100)//camera shake when plane is too fast
            {
                camTransform.localPosition = originalCamPos + UnityEngine.Random.insideUnitSphere * shakeAmount;
                planeTransform.localPosition = new Vector3(0, camTransform.localPosition.y - 0.6f, camTransform.localPosition.z - 0.786f);
                enginestatus_text.text = "Reduce Throttle";
                enginestatus_text_2.text = "";
                rpmtime += Time.deltaTime;
                if (rpmtime > 5)
                {
                    camTransform.localPosition = originalCamPos;
                    planeTransform.localPosition = originalPlanePos;
                    enginestatus_text.text = "";
                    enginestatus_text_2.text = "Burned";
                    while (throttle >= 0) throttle -= Time.deltaTime;
                    transform.position += transform.forward * Time.deltaTime * throttle;
                    enginestatus_text_3.text = "";
                    isCrashed = true;
                    explosion.Play();
                    fire.Play();
                    start++;
                    rpmtime = 0;
                }
            }
            else
            {
                camTransform.localPosition = originalCamPos;
                planeTransform.localPosition = originalPlanePos;
            }
        }
    }
}
