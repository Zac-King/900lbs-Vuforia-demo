using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:         Zac King 
/// Description:    This script keeps track of the user Selected object, and allows the user to affect the 
///                 posistion, rotation, scale, color of the selected object, as well as spawn a new object 
/// </summary>

public class ARManipulateObject : MonoBehaviour
{
    public GameObject m_selectedObject;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if(m_placingObject)     // If in spawning phase
        {
            SpawningObject();   // 
        }

        else if (!m_selectingColor)
        {
            Translation();      // Drag selected to change it's position
            ScaleCheck();       // Pinch to change the scale of the selectedObject
            Rotation();         // Rotate selectedObject based of on camera view
        }
    }

    #region Scaling
    // Variables used for scaling
    Vector3 m_intialScale;
    float m_intialPinchDistance;
    
    void ScaleCheck()
    {
        if (Input.touchCount >= 2)
        {
            var pinch0 = Input.GetTouch(0); // Store 
            var pinch1 = Input.GetTouch(1);

            if (pinch0.phase == TouchPhase.Ended || pinch0.phase == TouchPhase.Canceled ||
                pinch1.phase == TouchPhase.Ended || pinch1.phase == TouchPhase.Canceled)
            {
                return; // Pinch has ended break and return
            }

            if (pinch0.phase == TouchPhase.Began || pinch1.phase == TouchPhase.Began)
            {
                m_intialScale = m_selectedObject.transform.localScale;                      // Store Scale
                m_intialPinchDistance = Vector2.Distance(pinch0.position, pinch1.position); // Store pinch posistions
            }
            else
            {
                var currentDistance = Vector2.Distance(pinch0.position, pinch1.position);
                if (Mathf.Approximately(m_intialPinchDistance, 0))
                {
                    return; // if movement is to minor, do nothing
                }
                var scaleInput = currentDistance / m_intialPinchDistance;
                m_selectedObject.transform.localScale = m_intialScale * scaleInput;     // Apply the new scale to the object
            }
        }
    }
    #endregion
    
    #region Rotation
    // Variables for Rotating
    Vector3 m_PrevPos = Vector3.zero;
    Vector3 m_PosDelta = Vector3.zero;
    void Rotation()
    {
        var touch = Input.GetTouch(0);      // store touch input
        Vector3 newTouchPosV3 = new Vector3(touch.position.x, touch.position.y, 0); // Update the new
        if (Input.touchCount == 1 && touch.phase == TouchPhase.Moved && !m_isTranslating)
        {
            m_PosDelta = newTouchPosV3 - m_PrevPos;
            
            if(Vector3.Dot(m_selectedObject.transform.up, Vector3.up) >= 0) //If it's facing up
            {
                // Using the Dot pruduct of the 2 Vector3, i can apply a rotation despite the camera's view angle, and have it make since
                m_selectedObject.transform.Rotate(m_selectedObject.transform.up, -Vector3.Dot(m_PosDelta, Camera.main.transform.right), Space.World);   // left or right rotation
            }
            else    // If object is facing down
            {
                m_selectedObject.transform.Rotate(m_selectedObject.transform.up, Vector3.Dot(m_PosDelta, Camera.main.transform.right), Space.World);   // left or right rotation
            }
            
            m_selectedObject.transform.Rotate(Camera.main.transform.right, Vector3.Dot(m_PosDelta, Camera.main.transform.up), Space.World); // up and down
        }

        m_PrevPos = newTouchPosV3;  // update previous position
    }
    #endregion

    #region Translation
    [Space(10)] // Variables for Translation
    [Header("Object Translation Variables")]
    public Transform m_planePoint;
    public Transform m_storedParent;
    bool m_isTranslating = false;
    Vector3 m_PrevPos_translation = Vector3.zero;
    Vector3 m_PosDelta_translation = Vector3.zero;
    void Translation()
    {
        if (Input.touchCount == 0)
        {
            m_isTranslating = false;
        }

        var touch = Input.GetTouch(0);
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray= Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100, LayerMask.GetMask("Interactable")))   // if the single touch raycasts to an object
            {
                if (hit.collider.gameObject)
                {
                    m_selectedObject = hit.collider.gameObject;             // Store SelectedObject
                    m_isTranslating = true;
                }
            }
        }

        if(m_isTranslating)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100, LayerMask.GetMask("PositionPlane")))
            {
                m_planePoint.position = hit.point;

                m_selectedObject.transform.parent = m_planePoint;
            }
            else
            {
                m_selectedObject.transform.parent = m_storedParent;
            }
        }
        else
        {
            m_selectedObject.transform.parent = m_storedParent;
        }
    }
    #endregion

    #region Color
    [Space(10)] // Variables for Color functions
    [Header("Color Variables")]
    public Slider m_colorSlider;
    public Gradient m_colorGradient;
    bool m_selectingColor;
    public void SetColor()
    {
        Color color = m_colorGradient.Evaluate(m_colorSlider.value);
        m_selectedObject.GetComponent<MeshRenderer>().material.color = color;
    }

    public void ToggleSettingColor()
    {
        m_selectingColor = !m_selectingColor;

        m_colorSlider.gameObject.SetActive(m_selectingColor);
    }
    #endregion

    #region Spawning
    [Space(10)] // Variables for Spawning Objects
    [Header("Spawn Object Variables")]
    public GameObject m_prefab;
    bool m_placingObject = true;
    void SpawningObject()
    {
        if(!m_placingObject)    // if we are not in a phase that allow us to place an object end
        {
            return;
        }

        var touch = Input.GetTouch(0);

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        // Usig the same plane we use for the translation function we can place a new object 
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100, LayerMask.GetMask("FloorPlane")))
        {
            GameObject go = Instantiate(m_prefab, hit.point, Quaternion.identity);
            go.transform.parent = m_storedParent;
            go.transform.localRotation = new Quaternion(0, 0, 0, 0);
            Material mat = new Material(Shader.Find("Standard"));
            go.GetComponent<MeshRenderer>().material = mat;
            m_selectedObject = go;
        }
        
        m_placingObject = false;
    }
    public void SetSpawnPrefab(GameObject go)
    {
        m_prefab = go;
    }
    public void AllowToSpawnNewObject()
    {
        m_placingObject = true;
    }
    #endregion

}
