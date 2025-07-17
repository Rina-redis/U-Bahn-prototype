using System.Collections.Generic;
using UnityEngine;

public class VehicleCombatBehaviourSystem : UnitController
{
    [SerializeField] private VehicleCombatSystemData data;
    public override UnitData UnitData => data;

    public bool useAbordagingController;
    public bool useChasingController;
    public bool useEscapingController;
    public bool useNoDriverController;

    public AbordagingVehicleController abordagingVehicleController;
    public ChasingVehicleController chasingVehicleController;
    public EscapingVehicleController escapingVehicleController;
    public NoDriverVehicleController noDriverVehicleController;
    private GameCombatManager gameCombatManager;
    [SerializeField] private List<Transform> passengerSeats; // has to be equall to passengersAmount in data

    private VehicleBehaviourController curController;
    private EnemyCombatBehaviourSystem[] enemies;

    void Awake()
    {
        base.Init();
    }

    void Start()
    {
        gameCombatManager = GameObject.Find("GameCombatManager").GetComponent<GameCombatManager>();
        if (data._passengersList == null || data.passangersAmount != data._passengersList.Count || passengerSeats == null || passengerSeats.Count != data.passangersAmount)
        {
            Debug.Log("Vehicle Passengers were wrong defined. Please take a look at SO and passengerSeats. They have to be equal to passengersAmount in data");
            ChangeCurrentBehaviour(noDriverVehicleController);
            return;
        }

        enemies = new EnemyCombatBehaviourSystem[data.passangersAmount];

        for (int i = 0; i < data.passangersAmount; i++)
        {
            GameObject go = Instantiate(data._passengersList[i]);
            go.transform.SetParent(passengerSeats[i]);
            go.transform.localPosition = Vector3.zero;
            enemies[i] = go.GetComponent<EnemyCombatBehaviourSystem>();
        }

        if (data.isVehicleToRangers)
        {
            PlayerMock playerMock = gameCombatManager.GetNearestPlayer(transform);
            chasingVehicleController.SetTarget(playerMock);
            ChangeCurrentBehaviour(chasingVehicleController);
        }
        else
        {
            Transform abordagePoint = gameCombatManager.GetApplicableAbordagePoint(transform);
            abordagingVehicleController.SetTarget(abordagePoint);
            ChangeCurrentBehaviour(abordagingVehicleController);
        }
    }

    private void ChangeCurrentBehaviour(VehicleBehaviourController controller)
    {
        if (curController != null)
        {
            curController.OnEndBehaviour();
        }
        curController = controller;
        controller.OnStartBehaviour();
    }

    void FixedUpdate()
    {
        if (curController == null)
        {
            Debug.LogError("I dont have needed behaviour controller, or I didnt get one yet");
            return;
        }
        curController.OnFixedUpdateBehave();
    }

    //---// AbordagingController //---//
    public void Abordage(Vector2 abordagingPoint)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && (enemies[i].EnemyType == EnemyType.MELEE || enemies[i].EnemyType == EnemyType.SCOUNDREL))
            {
                enemies[i].gameObject.transform.SetParent(null);
                enemies[i].VehicleEndedTheAbordageProcess();
            }
        }
        ChangeCurrentBehaviour(escapingVehicleController);
    }

    //---// ChasingController //---//
    public void TellRangersToAttack(PlayerMock playerMock)
    {
        Debug.Log("I am trying to tell the ranger, so that they could attack");
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].EnemyType == EnemyType.RANGED)
            {
                enemies[i].VehicleToldTheRangerToAttack(playerMock);
            }
        }
    }

    protected override void Die()
    {
        throw new System.NotImplementedException();
    }
}
