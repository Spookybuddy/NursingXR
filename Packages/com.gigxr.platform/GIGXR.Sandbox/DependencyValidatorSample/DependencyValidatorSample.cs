using GIGXR.Platform.Core.DependencyValidator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DependencyValidatorSample : MonoBehaviour
{
    [Header("Deleting this inspector reference will cause the Dependency Validator to fail a build")]
    [RequireDependency]
    [SerializeField] private Transform dependentTransform;
}
