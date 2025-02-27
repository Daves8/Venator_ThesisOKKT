﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wolf : MonoBehaviour
{
    public float hp;

    private Animator _animator;
    private NavMeshAgent _agent;
    private PlayerCharacteristics _playerCharact;

    private GameObject _player;
    private GameObject[] _hunters;

    public bool _agressive = false;
    private bool _die = false;

    private float _speedWalk = 2.0f;
    private float _speedRun = 5.0f;

    public AudioClip[] roarWolf;
    private AudioSource _audioSource;

    private GameObject[] _places;
    private Animals _animals;

    private bool _startCoroutine = false;
    private bool _startCoroutineW = false;
    private bool _startCoroutineE = false;
    private bool _walkCorout;
    private bool _attack;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _hunters = GameObject.FindGameObjectsWithTag("Hunter");
        _playerCharact = _player.GetComponent<PlayerCharacteristics>();

        _places = GameObject.FindGameObjectsWithTag("PlacesForWolf");
        _places[_places.Length - 1] = GameObject.FindGameObjectWithTag("DenWolf");
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = 0.2f;
        hp = 450;
        _animals = GameObject.FindGameObjectWithTag("Animal").GetComponent<Animals>();
        ++_animals.allAnimals["Wolf"];
        StartCoroutine(Healing());
    }

    void Update()
    {
        if (_die) { return; }
        if (hp <= 0)
        {
            --_animals.allAnimals["Wolf"];
            _die = true;
            _agressive = false;
            _playerCharact.isBattleAnimal = false;
            _agent.enabled = false;
            _animator.SetTrigger("Die");
            Invoke("Delete", 300.0f);
            return;
        }

        if (!_audioSource.isPlaying)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.clip = roarWolf[Random.Range(0, roarWolf.Length)];
            _audioSource.Play();
        }

        if (Vector3.Distance(transform.position, _player.transform.position) < SafetyDistance() || NearHunters())
        {
            _agressive = true;
            _agent.speed = _speedRun;
        }
        else
        {
            _agressive = false;
            _agent.speed = _speedWalk;
        }

        if (_agressive)
        {
            _agent.speed = _speedRun;
            if (!_playerCharact.allAnimals.Contains(gameObject))
            {
                _playerCharact.allAnimals.Add(gameObject);
            }
        }
        else
        {
            _agent.speed = _speedWalk;
            _playerCharact.allAnimals.Remove(gameObject);
        }

        if (_agent.velocity.magnitude > 0f)
        {
            if (_agressive)
            {
                _animator.SetBool("Walk", false);
                _animator.SetBool("Eat", false);
                _animator.SetBool("Run", true);
            }
            else
            {
                _animator.SetBool("Run", false);
                _animator.SetBool("Eat", false);
                _animator.SetBool("Walk", true);
            }
        }
        else
        {
            _animator.SetBool("Walk", false);
            _animator.SetBool("Run", false);
            _animator.SetBool("Eat", false);
        }

        if (_agressive && hp <= 250 && Vector3.Distance(transform.position, _places[_places.Length - 1].transform.position) > 7.5f) { RunAway(); }
        else if (_agressive) { Attack(); }
        else { Walking(); }
    }
    private void Attack()
    {
        _walkCorout = false;
        Transform enHit = EnemyForHit().transform;
        if (Vector3.Distance(transform.position, enHit.position) > 1.5f)
        {
            _attack = false;
            _agent.SetDestination(enHit.position);
        }
        else
        {
            _attack = true;
            if (!_startCoroutine)
            {
                StartCoroutine(Hit());
            }
        }
    }
    private Transform EnemyForHit()
    {
        Transform enfh = _player.transform;
        for (int i = 0; i < _hunters.Length; i++)
        {
            if (Vector3.Distance(transform.position, enfh.position) > Vector3.Distance(transform.position, _hunters[i].transform.position))
            {
                enfh = _hunters[i].transform;
            }
        }
        return enfh;
    }
    IEnumerator Hit()
    {
        _agent.isStopped = true;
        _startCoroutine = true;
        while (_attack)
        {
            _animator.SetTrigger("Attack");
            yield return new WaitForSeconds(Random.Range(2.0f, 2.7f));
        }
        _agent.isStopped = false;
        _startCoroutine = false;
    }

    private void Walking()
    {
        _walkCorout = true;
        if (!_startCoroutineW)
        {
            StartCoroutine(Walk());
        }
    }
    IEnumerator Walk()
    {
        _startCoroutineW = true;
        while (_walkCorout)
        {
            _agent.SetDestination(_places[Random.Range(0, _places.Length)].transform.position);
            yield return new WaitForSeconds(Random.Range(5f, 180f));
        }
        _startCoroutineW = false;
    }
    private void RunAway()
    {
        _attack = false;
        _agent.SetDestination(_places[_places.Length - 1].transform.position);
    }

    public void Agressive()
    {
        if (!_die)
        {
            if (NearHunters())
            {
                _agressive = true;
            }
            else
            {
                RunAway();
            }
        }
    }
    private bool NearHunters()
    {
        float distance = Random.Range(9f, 11f);
        for (int i = 0; i < _hunters.Length; i++)
        {
            if (Vector3.Distance(transform.position, _hunters[i].transform.position) < distance)
            {
                return true;
            }
        }
        return false;
    }
    private float SafetyDistance()
    {
        float distance = Random.Range(9f, 11f);
        if (_playerCharact.crouch)
        {
            return distance;
        }
        return distance * 2f;
    }
    IEnumerator Healing()
    {
        while (!_die)
        {
            if (hp < 250)
            {
                hp += 10;
            }
            yield return new WaitForSeconds(10f);
        }
    }
    private void Delete()
    {
        Destroy(gameObject);
    }
}