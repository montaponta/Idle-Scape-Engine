using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColliderHelper
{
    public void EnterCollider(Collider collider, Transform sender, Collision collision = null);
    public void StayCollider(Collider collider, Transform sender, Collision collision = null);
    public void ExitCollider(Collider collider, Transform sender, Collision collision = null);
}
