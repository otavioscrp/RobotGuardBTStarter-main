using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //declaração de variaveis
    public Transform player;
    public Transform bulletSpawn;
    public Slider healthBar;
    public GameObject bulletPrefab;

    NavMeshAgent agent; // criação de NavMesh
    public Vector3 destination; // ponto de destino
    public Vector3 target;      // criação do alvo
    float health = 100.0f; // vida distribuida pelo agente
    float rotSpeed = 5.0f; // rotação do agente
    float visibleRange = 80.0f; //alcance de visão do agente
    float shotRange = 40.0f; // alcance do tiro do agente

    // ao inicio do projeto, atribuir o componente de navmesh criado, o alcance do tiro e o sistema de vida
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; 
        InvokeRepeating("UpdateHealth", 5, 0.5f);
    }

    // criação e posicionamento da barra de vida
    void Update()
    {
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
    }

    // criação da regeneração da vida do agente
    void UpdateHealth()
    {
        if (health < 100) 
            health++;
    }

    // dano e colisão do tiro do personagem
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "bullet")
        {
            health -= 10; 
        }
    }

    [Task] // referencia para o plugin
    public void PickRandomDestination() // movimentação aleatoria do agente
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)); 
        agent.SetDestination(dest); 
        Task.current.Succeed();
    }

    [Task]
    public void MoveToDestination()  // declaração de tempo para a movimentação do personagem
    
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time); 
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending) 
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public void PickDestination(int x, int z)
    {
        Vector3 dest = new Vector3(x, 0, z);
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public void TargetPlayer()
    {
        target = player.transform.position;
        Task.current.Succeed();
    }

    [Task] //atribuição da força e fisica do tiro do agente
    public bool Fire()
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab,
            bulletSpawn.transform.position, bulletSpawn.transform.rotation);

        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);

        return true;
    }

    [Task] // criação do metodo de direcionamento da visão do agente
    public void LookAtTarget()
    {
        Vector3 direction = target - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        if (Task.isInspected)
            Task.current.debugInfo = string.Format("angle={0}",
                Vector3.Angle(this.transform.forward, direction));
        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    bool SeePlayer()
    {//metodo de visão do agente para o jogador
        Vector3 distance = player.transform.position - this.transform.position;
        //entrada de colisão do raycast para as paredes
        RaycastHit hit;
        bool seeWall = false;
        Debug.DrawRay(this.transform.position, distance, Color.red);
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }
        if (Task.isInspected) Task.current.debugInfo = string.Format("wall={0}", seeWall);
        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }

    [Task] bool Turn(float angle)
    {
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) *
            this.transform.forward;
        target = p;
        return true; }
}

