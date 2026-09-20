using System;
using UnityEngine;
using DollShop.Core;

namespace DollShop.View
{
    /// <summary>수선 미니게임 공통 베이스 클래스.</summary>
    public abstract class MinigameBase : MonoBehaviour
    {
        // ── 상태 ──────────────────────────────────────────────
        protected bool  IsActive   { get; private set; } = false;
        protected float ElapsedSec { get; private set; } = 0f;
        protected float Accuracy   { get; protected set; } = 100f;

        protected OrderId _orderId;

        // ── 이벤트 ────────────────────────────────────────────
        public event Action<RepairInput> OnCompleted;
        public event Action              OnAborted;

        // ── 공개 API ──────────────────────────────────────────
        public virtual void Begin(OrderId orderId)
        {
            _orderId   = orderId;
            IsActive   = true;
            ElapsedSec = 0f;
            Accuracy   = 100f;
            gameObject.SetActive(true);
            OnBegin();
        }

        public void Abort()
        {
            if (!IsActive) return;
            IsActive = false;
            gameObject.SetActive(false);
            OnAbort();
            OnAborted?.Invoke();
        }

        // ── 구현 의무 ─────────────────────────────────────────
        protected abstract void OnBegin();
        protected abstract void OnAbort();

        protected virtual void Update()
        {
            if (!IsActive) return;
            ElapsedSec += Time.deltaTime;
        }

        protected void Complete()
        {
            if (!IsActive) return;
            IsActive = false;
            gameObject.SetActive(false);

            var input = new RepairInput(Accuracy, ElapsedSec, false);
            TimeService.SpendRepairTicks(ElapsedSec);
            OnCompleted?.Invoke(input);
        }
    }
}
