using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Conversation
{
    public Hobby hobby;
    public List<Character> participants = new();
    public float duration = 1f;           // 对话持续时间（秒）
    public Coroutine process;

    #region 核心逻辑
    public Conversation(Hobby hobby,Character owner)
    {
        this.hobby = hobby;
        participants = new() { owner };
    }

    // 3. 技能检查

    // 开始对话流程
    public IEnumerator Process()
    {
        while (true)
        {
            SettleAccount();
            yield return new WaitForSeconds(duration);
        }
    }
    private void SettleAccount()
    {
        for (int i = 0; i < participants.Count; i++)
        {
            for (int j = i + 1; j < participants.Count; j++)
            {
                Community.hobbyTopicScore.ModifyHobbyTopicScore(participants[i], participants[j], hobby, 1f);
                Community.affinity.ModifyAffinity(participants[i], participants[j], 1f);
            }
        }
    }

    // 获取当前话题（根据配置生成）
    private Hobby GetCurrentHobby(NPC npc)
    {
        Device device = npc.action.target as Device;
        return device.hobby;
    }

    public void Add(Character participant)
    {
        if (!participants.Contains(participant))
            participants.Add(participant);
        if (participants.Count == 2)
            process = Community.StartConversation(this);
    }
    public void Remove(Character participant)
    {
        participants.Remove(participant);
        if (participants.Count == 1)
            StopConversation();
        if (participants.Count == 0)
            EndConversation();
    }


    private void StopConversation()
    {
        Community.StopConversation(this);
    }

    // 结束对话
    private void EndConversation()
    {
        
    }
     

    #endregion

}
