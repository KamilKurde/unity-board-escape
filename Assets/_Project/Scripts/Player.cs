using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public enum EqState
{
    CanPlace,
    CanTake,
    CantPlace,
    NoTile,
    NoEq
}

public class Player : MonoBehaviour
{
    internal float shortAnimTime = GameManager.shortAnimationLenght;
    internal float mediumAnimTime = GameManager.mediumAnimationLenght;

    [SerializeField] private ParticleSystem _particleSystem;
    private bool _particlesWerePlayed = false;
    
    [SerializeField, Range(0.1f, 10f)] private float speed;
    
    [SerializeField] private CharacterController controller;
    
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    private bool _audioIsActive = false;
    [Range(0.01f, 5f)]public float volumeChangeTime;

    [Header("Rotation properties")] 
    [SerializeField, Range(1f, 50f)] private float rotationSpeed = 20f;
    [SerializeField, Range(0.01f, 1f)] private float swingPower = 0.3f;
    public EqState state = EqState.NoEq;
    private IPlaceable _placeable;
    private IInteractable _interactable = null;
    private PlaceTile _placeTile = null;

    private Vector3 _movementDirection = Vector3.zero;
    // Variable ot keep last direction (for use when player is no longer moving)
    private Vector3 _lastMovementDirection = Vector3.forward;
    private Vector3 _playerVelocity;

    private bool _isPaused = false;

    [HideInInspector] public bool levelFinished = false;

    private void Awake()
    {
        GameManager.sceneStartTime = Time.time;
    }

    private void Start()
    {
        GameManager.player = this;
        controller.Move(Vector3.up * 8f);
        audioSource.volume = 0f;
    }

    private void Update()
    {
        HandleVolumeChange(Time.deltaTime);
        if (levelFinished)
        {
            _audioIsActive = false;
            _movementDirection = Vector3.zero;
        }
        Move();
    }

    // Method invoked by Player Input Component when input associated with movement changed
    public void OnMovementInput(InputAction.CallbackContext context)
    {
        var inputMovement = context.ReadValue<Vector2>();
        _movementDirection = new Vector3(inputMovement.x, _movementDirection.y, inputMovement.y);
    }

    public void OnInteractionInput(InputAction.CallbackContext context)
    {
        if (levelFinished || !context.started)
        {
            return;
        }
        _interactable?.Interact();
    }

    private void UpdateEqState()
    {
        if (_placeTile == null)
        {
            state = EqState.NoTile;
            return;
        }
        
        if (_placeable != null && !_placeTile.HasPlaceable)
        {
            state = EqState.CanPlace;
            return;
        }

        if (_placeable == null && _placeTile.HasPlaceable)
        {
            state = EqState.CanTake;
            return;
        }

        if (_placeable != null && _placeTile.HasPlaceable)
        {
            state = EqState.CantPlace;
            return;
        }
    }

    public void OnPlaceInput(InputAction.CallbackContext context)
    {
        if (!context.started || _placeTile == null || levelFinished)
        {
            return;
        }

        // If both player and tile don't have placeable
        if (_placeable == null && !_placeTile.HasPlaceable)
        {
            return;
        }

        // if both player and tile do have placeable
        if (_placeable != null && _placeTile.HasPlaceable)
        {
            return;
        }

        if (_placeable == null && _placeTile.HasPlaceable)
        {
            _placeable = _placeTile.placeable;
            _placeTile.placeable = null;
            _placeable.Hide();
            UpdateEqState();
        }
        else if (_placeable != null && !_placeTile.HasPlaceable)
        {
            _placeTile.SetPlaceable(_placeable);
            _placeable = null;
            UpdateEqState();
        }
    }

    public void SetPauseState(bool state)
    {
        if (!state)
        {
            GameManager.uiManager.settingsScript.ChangeVisibilityTo(false);
        }
        _isPaused = state;
        GameManager.uiManager.uiGroup.DOFade(_isPaused ? 0f : 1f, shortAnimTime).SetUpdate(true);
        GameManager.uiManager.pauseGroup.DOFade(_isPaused ? 1f : 0f, shortAnimTime).SetUpdate(true);
        GameManager.uiManager.pauseGroup.interactable = _isPaused;
        GameManager.uiManager.pauseGroup.blocksRaycasts = _isPaused;
        audioSource.enabled = !state;
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void OnPauseEnter(InputAction.CallbackContext context)
    {
        if (!context.started || levelFinished)
        {
            return;
        }
        _isPaused = !_isPaused;
        SetPauseState(_isPaused);
    }

    public void OnContinueEnter(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }
        GameManager.uiManager.OnContinueButtonClicked();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (levelFinished)
        {
            return;
        }
        var collidedInteractable = other.GetComponent<IInteractable>();
        if (collidedInteractable != null)
        {
            _interactable = collidedInteractable;
        }

        var collidedTile = other.GetComponent<PlaceTile>();
        if (collidedTile != null)
        {
            _placeTile = collidedTile;
        }
        UpdateEqState();
    }

    private void OnTriggerExit(Collider other)
    {
        if (levelFinished)
        {
            return;
        }
        if (other.GetComponent<IInteractable>() == _interactable)
        {
            _interactable = null;
        }

        if (other.GetComponent<PlaceTile>() == _placeTile)
        {
            _placeTile = null;
        }
        UpdateEqState();
    }

    private void HandleVolumeChange(float time)
    {
        var volumeChangeStep = 1f / volumeChangeTime * time;
        var currentVolume = audioSource.volume;
        var targetVolume = _audioIsActive ? 1f : 0f;
        if (currentVolume < targetVolume)
        {
            audioSource.volume += volumeChangeStep;
            if (audioSource.volume > targetVolume)
            {
                audioSource.volume = targetVolume;
            }
        }
        else if (currentVolume > targetVolume)
        {
            audioSource.volume -= volumeChangeStep;
            if (audioSource.volume < targetVolume)
            {
                audioSource.volume += volumeChangeStep;
            }
        }
    }

    private IEnumerator StopParticles()
    {
        yield return 2f;
        _particleSystem.Stop();
    }

    private void Move()
    {
        // New rotation for the character
        Quaternion newRotation;

        // If object is in move
        if(_movementDirection != Vector3.zero)
        {
            // Change rotation according to character movement direction + swing
            newRotation = Quaternion.LookRotation(_movementDirection + Vector3.up * -swingPower);
            controller.Move(_movementDirection * (speed * Time.deltaTime));
            _lastMovementDirection = _movementDirection;
            _audioIsActive = true;
        }
        else
        {
            newRotation = Quaternion.LookRotation(_lastMovementDirection);
            _audioIsActive = false;
        }

        if (controller.isGrounded && _playerVelocity.y < 0)
        {
            if (!_particlesWerePlayed)
            {
                _particlesWerePlayed = true;
                _particleSystem.Play();
                StartCoroutine(StopParticles());
            }
            _playerVelocity.y = 0f;
            animator.enabled = false;
        }

        _playerVelocity.y = Physics.gravity.y * Time.deltaTime;

        if (!controller.isGrounded)
        {
            controller.Move(_playerVelocity);
        }

        // Apply rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);

    }

    public bool HasInteractable()
    {
        return _interactable != null;
    }
}