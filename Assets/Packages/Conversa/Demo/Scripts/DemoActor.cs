using Conversa.Runtime;
using UnityEngine;

namespace Conversa.Demo
{
    [CreateAssetMenu(fileName = "Actor", menuName = "Conversa/Demo/Actor", order = 0)]
    [System.Serializable]
    public class DemoActor : Actor
    {
        [SerializeField] private Sprite avatar;
        public Sprite Avatar => avatar;
    }
}