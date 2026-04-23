// =====================================================
// Test.cs - 開発・動作確認用のオブジェクト移動スクリプト
// 使い方: 確認したいオブジェクトにアタッチするだけ。
//         本番シーンでは使用しないこと。
// =====================================================
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float speed = 5.0f;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}

//githubdesktop 確認