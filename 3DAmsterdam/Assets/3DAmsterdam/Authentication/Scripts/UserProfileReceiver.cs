using Cdm.Authentication;
using TMPro;
using UnityEngine;

namespace Netherlands3D.Authentication.Sample
{
    public class UserProfileReceiver : MonoBehaviour
    {
        public TMP_Text textField;
        public Session session;

        private void OnEnable()
        {
            session.OnUserInfoReceived.AddListener(OnUserInfo);
        }

        private void OnDisable()
        {
            session.OnUserInfoReceived.RemoveListener(OnUserInfo);
        }

        private void OnUserInfo(IUserInfo userInfo)
        {
            textField.SetText(userInfo.name);
        }
    }
}
