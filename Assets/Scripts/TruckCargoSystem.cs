using TMPro;
using UnityEngine;

public class TruckCargoSystem : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] public Transform cargoHoldPoint;
    [SerializeField] private int maxCargoCount = 1;
    private CargoPickup cargoPickup;
    public float massCargo = 0f;
    public TaskText taskText; // !!!!

    public Transform currentCargo = null;

    public bool CanPickupCargo() => currentCargo == null;

    public void LoadCargo(Transform cargo)
    {
        if (currentCargo != null) return;

        currentCargo = cargo;
        massCargo = cargoPickup.massCargo;
       
        taskText.isCargoPicked = true; //
        taskText.UpdateTaskText();     //
        Debug.Log("Груз успешно загружен в кузов");
    }

    public void UnloadCurrentCargo()
    {
        if (currentCargo == null) return;

        currentCargo.SetParent(null);

        currentCargo = null;
        taskText.isCargoPicked = false;  //
        taskText.UpdateTaskText();       //
        Debug.Log("Груз выгружен");
    }

    public bool HasCargo() => currentCargo != null;
}