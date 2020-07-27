using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Sharing
{
    [Serializable]
    public struct ServerReturn
    {
        public string sceneId;
        public string sceneUrl;
        public ModelUploadToken[] modelUploadTokens;
    }

    [Serializable]
    public struct ModelUploadToken{
        public int layerID;
        public string token;
    }
}