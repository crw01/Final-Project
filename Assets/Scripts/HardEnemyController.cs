using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardEnemyController : MonoBehaviour
{
    public float speed;
    public bool vertical;
    public float changeTime = 3.0f;

    Rigidbody2D rb;
    float timer;
    int direction = 1;
    bool broken = true;
    
    Animator animator;
    public AudioSource musicSource;
    public AudioClip fixedSound;

    public ParticleSystem smokeEffect;

    RubyController rubyController;

    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = changeTime;
        animator = GetComponent<Animator>();

        rubyController = GameObject.FindWithTag("RubyController").GetComponent<RubyController>();

        GameObject rubyControllerObject = GameObject.FindWithTag("RubyController");
 //this line of code finds the RubyController script by looking for a "RubyController" tag on Ruby

    }

    void Update()
    {
        //remember ! inverse the test, so if broken is true !broken will be false and return wont be executed.
        if(!broken)
        {
            return;
        }
        
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            direction = -direction;
            timer = changeTime;
        }
    }
    
    void FixedUpdate()
    {
        //remember ! inverse the test, so if broken is true !broken will be false and return won’t be executed.
        if(!broken)
        {
            return;
        }
        
        Vector2 position = rb.position;
        
        if (vertical)
        {
            position.y = position.y + Time.deltaTime * speed * direction;
            animator.SetFloat("Move X", 0);
            animator.SetFloat("Move Y", direction);
        }
        else
        {
            position.x = position.x + Time.deltaTime * speed * direction;
            animator.SetFloat("Move X", direction);
            animator.SetFloat("Move Y", 0);
        }
        
        rb.MovePosition(position);
    }
    
    void OnCollisionEnter2D(Collision2D other)
    {
        RubyController player = other.gameObject.GetComponent<RubyController >();

        if (player != null)
        {
            player.ChangeHealth(-2);
        }
    }
    
    //Public because we want to call it from elsewhere like the projectile script
    public void Fix()
    {
        broken = false;
        rb.simulated = false;
        smokeEffect.Stop();
        animator.SetTrigger("Fixed");
        rubyController.ChangeCount(1);
        musicSource.PlayOneShot(fixedSound);
    }
}
