using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Utility.Extend.UniTaskExtend
{
    public static class UniTaskExtend
    {
        /// <summary>
        /// 用于在满足条件时执行方法
        /// </summary>
        /// <param name="action">action是要执行的方法</param> 
        /// <param name="t">t是要传入的参数</param>
        /// <param name="condition">condition是条件</param>
        /// <param name="token">token是取消令牌</param> 
        /// <typeparam name="T">T是t的类型</typeparam> 
        public static async UniTask DoWhen<T>(Action action, T t, Func<T, bool> condition,
            CancellationToken? token = null)
        {
            token?.ThrowIfCancellationRequested();
            await UniTask.WaitUntil(() => condition.Invoke(t), cancellationToken: token ?? CancellationToken.None);

            action();
        }


        /// <summary>
        /// 绑定target的值的变化，当target的值发生变化时执行action
        /// </summary>
        /// <param name="target">target必须是引用类型</param>
        /// <param name="getValue">getValue是获取target的值的委托</param>
        /// <param name="action">action是当target的值发生变化时执行的方法</param>
        /// <param name="token">token是取消令牌</param>
        /// <typeparam name="T">T是target的类型</typeparam>
        /// <typeparam name="TU">TU是target的值的类型</typeparam>
        public static async UniTask BindValueChange<T, TU>(T target, Func<T, TU> getValue, Action action,
            CancellationToken? token = null) where T : class
        {
            token?.ThrowIfCancellationRequested();
            await UniTask.WaitUntilValueChanged(target, getValue, cancellationToken: token ?? CancellationToken.None);
            action();
            BindValueChange(target, getValue, action, token).Forget();
        }

        /// <summary>
        /// 等待seconds秒
        /// </summary>
        /// <param name="seconds">秒数</param>
        /// <param name="token">取消令牌</param>
        public static async UniTask WaitForSeconds(float seconds, CancellationToken? token = null)
        {
            token?.ThrowIfCancellationRequested();
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: token ?? CancellationToken.None);
        }
    }
}