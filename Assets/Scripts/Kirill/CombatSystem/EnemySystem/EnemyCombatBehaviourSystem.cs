using UnityEngine;
public class EnemyCombatBehaviourSystem : UnitController
{
    public override UnitData UnitData => data;
    [SerializeField] private EnemyCombatSystemData data;
    public bool useRidingController;
    public bool useAttackingMeleeController;
    public bool useAttackingRangedController;
    public bool useEscapingController;
    public bool useHauntingController;
    public bool useStealingController;

    public RidingBehaviourController ridingBehaviourController;
    public AttackingMeleeBehaviourController attackingMeleeBehaviourController;
    public AttackingRangedBehaviourController attackingRangedBehaviourController;
    public EscapingBehaviourController escapingBehaviourController;
    public HauntingBehaviourController hauntingBehaviourController;
    public StealingBehaviourController stealingBehaviourController;

    [HideInInspector] public EnemyType EnemyType { get => data.enemyType; }

    [SerializeField] private PlayerMock playerMock;
    private GameCombatManager gameCombatManager;

    private CombatBehaviourController curController;

    void Awake()
    {
        base.Init();
    }

    void Start()
    {
        gameCombatManager = GameObject.Find("GameCombatManager").GetComponent<GameCombatManager>();
        ChangeCurrentBehaviour(ridingBehaviourController);

        //ChangeCurrentBehaviour(escapingBehaviourController);


        /*attackingMeleeBehaviourController.SetTarget(playerMock);
        ChangeCurrentBehaviour(attackingMeleeBehaviourController);*/

        //ChangeCurrentBehaviour(hauntingBehaviourController);

        /*attackingRangedBehaviourController.SetTarget(playerMock);
        ChangeCurrentBehaviour(attackingRangedBehaviourController);*/
    }

    private void ChangeCurrentBehaviour(CombatBehaviourController controller)
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
            Debug.LogError("I dont have needed behaviour controller");
            return;
        }
        curController.OnFixedUpdateBehave();
    }


    //---//RidingBehaviourController //---//
    public void VehicleToldTheRangerToAttack(PlayerMock target) // Applicable only to Rangers
    {
        Debug.Log("I set a ranger to attack");
        attackingRangedBehaviourController.SetTarget(target);
        ChangeCurrentBehaviour(attackingRangedBehaviourController);
    }

    public void VehicleEndedTheAbordageProcess() // Applicable only to Melee and Scoundrels
    {
        EctsContainer container = gameCombatManager.GetNearestContainer(transform);
        hauntingBehaviourController.SetTarget(container);
        ChangeCurrentBehaviour(hauntingBehaviourController);
    }


    //---// AttackingMeleeBehaviourController //---//
    public void MeleeLoseTarget()
    {
        ChangeCurrentBehaviour(hauntingBehaviourController);
    }

    //---// AttackingRangedBehaviourController //---//
    public void RangedLoseTarget()
    {
        ChangeCurrentBehaviour(escapingBehaviourController);
    }


    //---// HauntingBehaviourController //---//
    public void StartStealing(EctsContainer container)
    {
        stealingBehaviourController.SetTarget(container);
        ChangeCurrentBehaviour(stealingBehaviourController);
    }


    //---// StealingBehaviourController //---//
    public void BoxWasStolen(EctsContainer container)
    {
        container.Steal();
        Debug.Log("I increment enemy score for stealing box with ects");
        ChangeCurrentBehaviour(escapingBehaviourController);
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }

    public override void Hurt(float damage, UnitController attacker)
    {
        base.Hurt(damage, attacker);
        transform.Translate((transform.position - attacker.transform.position) * 0.3f);
    }
}
