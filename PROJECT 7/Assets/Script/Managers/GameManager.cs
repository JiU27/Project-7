using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 游戏管理器 - 全局游戏状态管理
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("管理器")]
        public DeckManager deckManager;

        [Header("游戏状态")]
        public bool isInBattle = false;

        private void Awake()
        {
            // 单例模式
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            Debug.Log("游戏管理器初始化");
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle()
        {
            isInBattle = true;
            Debug.Log("开始战斗");
            // TODO: 加载战斗场景
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndBattle(bool victory)
        {
            isInBattle = false;
            
            if (victory)
            {
                Debug.Log("战斗胜利");
                // TODO: 战斗胜利奖励
            }
            else
            {
                Debug.Log("战斗失败");
                // TODO: 战斗失败处理
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("退出游戏");
            Application.Quit();
        }
    }
}

