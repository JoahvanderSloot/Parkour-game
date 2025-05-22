using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Extensions {
    public static class BDebug {
        #region DebugDrawLines
            #region Box
            [Conditional("UNITY_EDITOR")]
            public static void DrawWireBox(BoxCollider boxCollider, float duration = 1f, Color color = default) {
                if (color == default)
                    color = Color.red;
                
                var boxTransform = boxCollider.transform;
                var pos = boxTransform.TransformPoint(boxCollider.center);
                var halfExtents = new Vector3(
                    boxCollider.size.x * boxCollider.transform.lossyScale.x * 0.5f,
                    boxCollider.size.y * boxCollider.transform.lossyScale.y * 0.5f,
                    boxCollider.size.z * boxCollider.transform.lossyScale.z * 0.5f
                );
                var rotation = boxTransform.rotation;
                
                DrawWireBox(pos, halfExtents, rotation, duration, color);
            }

            [Conditional("UNITY_EDITOR")]
            public static void DrawWireBox(Vector3 position, Vector3 halfExtents, float duration = 1f, Color color = default) {
                DrawWireBox(position, halfExtents, Quaternion.identity, duration, color);
            }
                
            [Conditional("UNITY_EDITOR")]
            public static void DrawWireBox(Vector3 position, Vector3 halfExtents, Quaternion rotation, float duration = 1f, Color color = default) {
                if (color == default)
                    color = Color.red;
                
                var p0 = position + rotation * new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
                var p1 = position + rotation * new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
                var p2 = position + rotation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
                var p3 = position + rotation * new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);

                var p4 = position + rotation * new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
                var p5 = position + rotation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
                var p6 = position + rotation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);
                var p7 = position + rotation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);

                Debug.DrawLine(p0, p1, color, duration);
                Debug.DrawLine(p1, p2, color, duration);
                Debug.DrawLine(p2, p3, color, duration);
                Debug.DrawLine(p3, p0, color, duration);

                Debug.DrawLine(p4, p5, color, duration);
                Debug.DrawLine(p5, p6, color, duration);
                Debug.DrawLine(p6, p7, color, duration);
                Debug.DrawLine(p7, p4, color, duration);

                Debug.DrawLine(p0, p4, color, duration);
                Debug.DrawLine(p1, p5, color, duration);
                Debug.DrawLine(p2, p6, color, duration);
                Debug.DrawLine(p3, p7, color, duration);
            }

            [Conditional("UNITY_EDITOR")]
            public static void DrawWireBox(Vector3 position, float size, float duration = 1f, Color color = default) {
                DrawWireBox(position, size, Quaternion.identity, duration, color);
            }
            
            [Conditional("UNITY_EDITOR")]
            public static void DrawWireBox(Vector3 position, float size, Quaternion rotation, float duration = 1f, Color color = default) {
                if (color == default)
                    color = Color.red;
                
                var halfExtents = new Vector3(size, size, size) * 0.5f;
                
                DrawWireBox(position, halfExtents, rotation, duration, color);
            }
            #endregion

            #region  Cross

            [Conditional("UNITY_EDITOR")]
            public static void DrawCross(Vector3 position, float radius, float duration = 1, Color color = default) {
                DrawCross(position, radius, Quaternion.identity, duration, color);
            }
        
            [Conditional("UNITY_EDITOR")]
            public static void DrawCross(Vector3 position, float radius, Quaternion rotation, float duration = 1, Color color = default) {
                if (color == default)
                    color = Color.red;
            
                radius *= 0.5f;
            
                Debug.DrawLine(position + rotation * Vector3.up * radius, position + rotation * Vector3.down * radius, color, duration);
                Debug.DrawLine(position + rotation * Vector3.left * radius, position + rotation * Vector3.right * radius, color, duration);
                Debug.DrawLine(position + rotation * Vector3.forward * radius, position + rotation * Vector3.back * radius, color, duration);
            }

            #endregion

            #region Sphere

            [Conditional("UNITY_EDITOR")]
            public static void DrawWireSphere(SphereCollider collider, float duration = 1f, Color color = default, float circleStep = 10f) {
                if (color == default)
                    color = Color.red;
                
                var sphereTransform = collider.transform;
                var pos = sphereTransform.TransformPoint(collider.center);
                var radius = collider.radius * sphereTransform.lossyScale.x;
                var rotation = sphereTransform.rotation;
                
                DrawWireSphere(pos, radius, rotation, duration, color, circleStep);
            }
            
            [Conditional("UNITY_EDITOR")]
            public static void DrawWireSphere(Vector3 position, float radius, float duration = 1f, Color color = default, float circleStep = 10f) {
                DrawWireSphere(position, radius, Quaternion.identity, duration, color, circleStep);
            }

            [Conditional("UNITY_EDITOR")]
            public static void DrawWireSphere(Vector3 position, float radius, Quaternion rotation, float duration = 1f, Color color = default, float circleStep = 10f) {
                if (color == default)
                    color = Color.red;
                
                DrawCross(position, radius, rotation, duration, color);
                
                radius *= 0.5f;
                
                var previousPosition = position + rotation * Vector3.up * radius;
     
                for (float i = 0; i < 360; i += circleStep) {
                    float angle = i * Mathf.Deg2Rad;
                    Vector3 pointOnSphere = position + rotation * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                    
                    if (i == 0) {
                        previousPosition = pointOnSphere;
                        continue;
                    }
                    
                    Debug.DrawLine(previousPosition, pointOnSphere, color, duration);
                    previousPosition = pointOnSphere;
                }
                
                for (float i = 0; i < 360; i += circleStep) {
                    float angle = i * Mathf.Deg2Rad;
                    Vector3 pointOnSphere = position + rotation * new Vector3(0, Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    
                    if (i == 0) {
                        previousPosition = pointOnSphere;
                        continue;
                    }
                    
                    Debug.DrawLine(previousPosition, pointOnSphere, color, duration);
                    previousPosition = pointOnSphere;
                }
                
                for (float i = 0; i < 360; i += circleStep) {
                    float angle = i * Mathf.Deg2Rad;
                    Vector3 pointOnSphere = position + rotation * new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
                    
                    if (i == 0) {
                        previousPosition = pointOnSphere;
                        continue;
                    }
                    
                    Debug.DrawLine(previousPosition, pointOnSphere, color, duration);
                    previousPosition = pointOnSphere;
                }
            }
            
            // ReSharper disable once CognitiveComplexity
            [Conditional("UNITY_EDITOR")]
            public static void DrawSphere(Vector3 position, float radius, float duration = 1f, Color color = default, float circleStep = 30f, float sphereStep = 5f) {
                if (color == default)
                    color = Color.red;
                
                radius *= 0.5f;
                
                var previousPosition = position + Vector3.up * radius;
     
                for (float i = 0; i < 360; i += sphereStep) {
                    var rot = Quaternion.Euler(0, i, 0);

                    for (float j = 0; j < 360; j += circleStep) {
                        float angle = j * Mathf.Deg2Rad;
                        Vector3 pointOnSphere = position + rot * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                        
                        if (i == 0) {
                            previousPosition = pointOnSphere;
                            continue;
                        }
                        
                        Debug.DrawLine(previousPosition, pointOnSphere, color, duration);
                        previousPosition = pointOnSphere;
                    }
                }
                
                for (float i = 0; i < 360; i += sphereStep) {
                    var rot = Quaternion.Euler(i, 0, 0);

                    for (float j = 0; j < 360; j += circleStep) {
                        float angle = j * Mathf.Deg2Rad;
                        Vector3 pointOnSphere = position + rot * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                        
                        if (i == 0) {
                            previousPosition = pointOnSphere;
                            continue;
                        }
                        
                        Debug.DrawLine(previousPosition, pointOnSphere, color, duration);
                        previousPosition = pointOnSphere;
                    }
                }
                
                for (float i = 0; i < 360; i += sphereStep) {
                    var rot = Quaternion.Euler(0, 0, i);

                    for (float j = 0; j < 360; j += circleStep) {
                        float angle = j * Mathf.Deg2Rad;
                        Vector3 pointOnSphere = position + rot * new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
                        
                        if (i == 0) {
                            previousPosition = pointOnSphere;
                            continue;
                        }
                        
                        Debug.DrawLine(previousPosition, pointOnSphere, color, duration);
                        previousPosition = pointOnSphere;
                    }
                }
            }
            #endregion
        #endregion
    }
}