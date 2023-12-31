﻿using AudioModule.Contracts;
using Savidiy.Utils;
using UnityEngine;

namespace MainModule
{
    public sealed class PlayerInputShooter
    {
        private readonly TickInvoker _tickInvoker;
        private readonly PlayerHolder _playerHolder;
        private readonly GameStaticData _gameStaticData;
        private readonly BulletFactory _bulletFactory;
        private readonly BulletHolder _bulletHolder;
        private readonly MobileInput _mobileInput;
        private readonly IAudioPlayer _audioPlayer;
        private float _cooldownTimer;

        public PlayerInputShooter(TickInvoker tickInvoker, PlayerHolder playerHolder, GameStaticData gameStaticData,
            BulletFactory bulletFactory, BulletHolder bulletHolder, MobileInput mobileInput, IAudioPlayer audioPlayer)
        {
            _tickInvoker = tickInvoker;
            _playerHolder = playerHolder;
            _gameStaticData = gameStaticData;
            _bulletFactory = bulletFactory;
            _bulletHolder = bulletHolder;
            _mobileInput = mobileInput;
            _audioPlayer = audioPlayer;
        }

        public void ActivatePlayerControls()
        {
            _tickInvoker.Updated -= OnUpdated;
            _tickInvoker.Updated += OnUpdated;
        }

        public void DeactivatePlayerControls()
        {
            _tickInvoker.Updated -= OnUpdated;
        }

        private float GetDeltaTime()
        {
            return _tickInvoker.DeltaTime;
        }

        private void OnUpdated()
        {
            UpdateTimer();

            if (CanShoot())
                CreateBullets();
        }

        private void UpdateTimer()
        {
            if (_cooldownTimer > 0)
                _cooldownTimer -= GetDeltaTime();
        }

        private bool CanShoot()
        {
            bool isKeyPressed = _gameStaticData.ShootKeys.IsAnyKeyPressed() || _mobileInput.IsFirePressed;
            return _cooldownTimer <= 0 && isKeyPressed;
        }

        private void CreateBullets()
        {
            _cooldownTimer = _gameStaticData.ShootCooldown;

            Player player = _playerHolder.Player;
            if (player.ActiveGuns.Count == 0)
            {
                _audioPlayer.PlayOnce(SoundId.PlayerEmptyFire);
                return;
            }

            foreach (GunType gunType in player.ActiveGuns)
            {
                Vector3 gunPosition = player.GetGunPosition(gunType);
                Vector3 gunDirection = player.GetGunDirection(gunType);
                Bullet bullet = _bulletFactory.CreateBullet(gunPosition, gunDirection, true);
                _bulletHolder.AddBullet(bullet);
            }

            _audioPlayer.PlayOnce(SoundId.PlayerFire);
        }
    }
}