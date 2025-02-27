﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rat : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private ImportantBuildings _importBuild;

    public AudioClip ratAudio;
    private AudioSource _audioSource;

    private Animals _animals;

    private bool _die;
    private bool _coroutStarted = false;

    void Start()
    {
        _animals = GameObject.FindGameObjectWithTag("Animal").GetComponent<Animals>();
        //_animals.allAnimals.Add("Rat", ++_animals.allAnimals["Rat"]);
        ++_animals.allAnimals["Rat"];
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _die = false;
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = 0.01f;
        //StartCoroutine(Walk());
    }

    private void Update()
    {
        if (_die)
        {
            return;
        }

        if (!_audioSource.isPlaying)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.clip = ratAudio;
            _audioSource.Play();
        }

        _agent.speed = 3.0f;
        if (!_coroutStarted)
        {
            _coroutStarted = true;
            StartCoroutine(Walk());
        }
        if (_agent.velocity.magnitude > 0f)
        {
            _animator.SetBool("Walk", true);
        }
        else
        {
            _animator.SetBool("Walk", false);
        }
    }

    IEnumerator Walk()
    {
        _importBuild = GameObject.FindGameObjectWithTag("BuildingsImportant").GetComponent<ImportantBuildings>();
        while (true)
        {
            _agent.SetDestination(_importBuild.allImportantBuildings[Random.Range(0, _importBuild.allImportantBuildings.Length)].transform.position);
            yield return new WaitForSeconds(Random.Range(5f, 180f));
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_die)
        {
            if (other.gameObject.tag == "Arrow")
            {
                Invoke("Death", 1f);
                _animator.SetTrigger("Die");
                _agent.enabled = false;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!_die)
        {
            if (other.gameObject.tag == "Sword")
            {
                Invoke("Death", 1f);
                _animator.SetTrigger("Die");
                _agent.enabled = false;
            }
            else if (other.gameObject.tag == "Knife")
            {
                Invoke("Death", 1f);
                _animator.SetTrigger("Die");
                _agent.enabled = false;
            }
        }
    }
    private void Death()
    {
        --_animals.allAnimals["Rat"];
        _animator.enabled = false;
        _die = true;
        --_animals.allAnimals["Rat"];
        GetComponent<CapsuleCollider>().enabled = false;
        _audioSource.enabled = false;
        transform.position -= new Vector3(0f, 0.1f, 0f);
        Invoke("Delete", 200f);
    }
    private void Delete()
    {
        Destroy(gameObject);
    }
}