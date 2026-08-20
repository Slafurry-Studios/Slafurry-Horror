// using UnityEngine;

// public class ItemRotator : MonoBehaviour
// {
//     public Transform examinePoint;
//     public float inspectDistance = 0.5f;
//     public float rotationSpeed = 100f;

//     private Transform item;
//     private Transform pivot;

//     private Transform originalParent;
//     private Vector3 originalLocalPosition;
//     private Quaternion originalLocalRotation;
//     private Vector3 originalLocalScale;

//     private Vector3 previousMousePosition;

//     public bool IsInspecting => item != null;

//     public void Inspect(GameObject target)
//     {
//         if (target == null || IsInspecting)
//             return;

//         item = target.transform;

//         // Simpan transform asli
//         originalParent = item.parent;
//         originalLocalPosition = item.localPosition;
//         originalLocalRotation = item.localRotation;
//         originalLocalScale = item.localScale;

//         // Cari titik tengah object
//         Bounds bounds = CalculateBounds(item);

//         // Buat pivot
//         GameObject pivotObject = new GameObject("InspectPivot");

//         pivot = pivotObject.transform;
//         pivot.position = bounds.center;
//         pivot.rotation = Quaternion.identity;

//         // Pivot mengikuti ExaminePoint
//         pivot.SetParent(examinePoint, true);

//         // Object masuk ke pivot
//         item.SetParent(pivot, true);

//         // Pindahkan ke depan ExaminePoint
//         pivot.localPosition = new Vector3(
//             0f,
//             0f,
//             inspectDistance
//         );
//     }

//     private void Update()
//     {
//         if (!IsInspecting)
//             return;

//         Rotate();
//     }

//     private void Rotate()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             previousMousePosition = Input.mousePosition;
//         }

//         if (Input.GetMouseButton(0))
//         {
//             Vector3 delta =
//                 Input.mousePosition - previousMousePosition;

//             float rotateX =
//                 delta.y * rotationSpeed * Time.deltaTime;

//             float rotateY =
//                 -delta.x * rotationSpeed * Time.deltaTime;

//             pivot.rotation =
//                 Quaternion.Euler(
//                     rotateX,
//                     rotateY,
//                     0f
//                 ) * pivot.rotation;

//             previousMousePosition = Input.mousePosition;
//         }
//     }

//     public void StopInspect()
//     {
//         if (!IsInspecting)
//             return;

//         item.SetParent(originalParent, true);

//         item.localPosition = originalLocalPosition;
//         item.localRotation = originalLocalRotation;
//         item.localScale = originalLocalScale;

//         Destroy(pivot.gameObject);

//         item = null;
//         pivot = null;
//     }

//     private Bounds CalculateBounds(Transform target)
//     {
//         Renderer[] renderers =
//             target.GetComponentsInChildren<Renderer>();

//         if (renderers.Length == 0)
//             return new Bounds(target.position, Vector3.zero);

//         Bounds bounds = renderers[0].bounds;

//         for (int i = 1; i < renderers.Length; i++)
//         {
//             bounds.Encapsulate(renderers[i].bounds);
//         }

//         return bounds;
//     }
// }