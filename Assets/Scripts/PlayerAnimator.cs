using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField] private Player _player;
    private Animator _animator;

    private const string IS_WALKING = "IsWalking";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.SetBool(IS_WALKING, _player.IsWalking());
    }

    private void Update()
    {
        if (!IsOwner) return;
        _animator.SetBool(IS_WALKING, _player.IsWalking());
    }
}
