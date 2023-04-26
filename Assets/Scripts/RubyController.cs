using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;
    public int maxHealth = 5;
    public int cogs;
    public int shrooms;
    
    public GameObject projectilePrefab;
    
    public int health { get { return currentHealth; }}
    int currentHealth;
    
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;
    
    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;
    
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);

    public AudioSource musicSource;
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip victorySound;
    public AudioClip lossSound;
    public AudioClip completeSound;

    public ParticleSystem healthIncreaseEffect;
    public ParticleSystem healthDecreaseEffect;

    private int count;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public GameObject loserTextObject;
    public TextMeshProUGUI cogsText;
    public TextMeshProUGUI shroomsText;

    bool gameOver;
    public static int level;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;

        musicSource= GetComponent<AudioSource>();

        count = 0;
        SetCountText();
        cogs = 4;
        SetCogsText();
        if (level == 2)
        {
            shrooms = 0;
            SetShroomsText();
        }

        winTextObject.SetActive(false);
        loserTextObject.SetActive(false);
        gameOver = false;

    }

    void SetCountText()
    {
        countText.text = "Robots Fixed: " + count.ToString() + "/5";
    }

    void SetCogsText()
    {
        cogsText.text = "Cogs: " + cogs.ToString();
    }

    void SetShroomsText()
    {
        shroomsText.text = "Shrooms: " + shrooms.ToString() + "/5";
    }
    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        
        Vector2 move = new Vector2(horizontal, vertical);

        if (level == 1)
        {
        if (count == 5)
        {
            musicSource.PlayOneShot(completeSound);
        }
        }

        if (level == 2)
        {            
            if (count==5 && shrooms>=5)
            {
                winTextObject.SetActive(true);
                gameOver = true;
                musicSource.PlayOneShot(completeSound);
                musicSource.Play();
                speed = 0f;
            }
        }



        if (health <= 0)
        {
            loserTextObject.SetActive(true);
            gameOver = true;
            musicSource.PlayOneShot(lossSound);
            speed = 0f;
        }

        if (Input.GetKey(KeyCode.R))
        {
            if (gameOver == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                speed = 0f;
            }
        }

        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }
        
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }
        
        if(Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));

            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (count >= 5)
                {
                    character.DisplayNextDialog();
                }

                else if (character != null)
                {
                    character.DisplayDialog();
                }  

                Level2NPC animal = hit.collider.GetComponent<Level2NPC>();
                if (animal != null)
                {
                    animal.DisplayDialog();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene("Level2");
            level = 2;        
        }
    }
    
    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            
            PlaySound(hitSound);

            Instantiate(healthDecreaseEffect,rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        }
        
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

        if (amount > 0)
        {
            Instantiate(healthIncreaseEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }
    }

    public void ChangeCount(int countAmount)
    {
        count += countAmount;
        countText.text = "Robots Fixed: " + count.ToString() + "/5";
    }
    
    public void ChangeAmmo(int cogsAmount)
    {
        cogs += cogsAmount;
        cogsText.text = "Cogs: " + cogs.ToString();
    }

    public void ChangeShrooms(int shroomsAmount)
    {
        shrooms += shroomsAmount;
        shroomsText.text = "Shrooms: " + shrooms.ToString() + "/5";
    }


    void Launch()
    {
        if (cogs >= 1)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(lookDirection, 300);

            animator.SetTrigger("Launch");

            PlaySound(throwSound);

            cogs = cogs - 1;
            SetCogsText();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ammo"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shroom"))
        {
            shrooms = shrooms + 1;
            SetShroomsText();
        }

        if (other.gameObject.CompareTag("Slow"))
        {
            speed = 1.0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Slow"))
        {
            speed = 3.0f;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        musicSource.PlayOneShot(clip);
    }
}