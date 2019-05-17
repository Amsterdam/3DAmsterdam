mergeInto(LibraryManager.library, {


  DownloadFile: function (uri, filename)
  {
      var sfilename = Pointer_stringify(filename);
      var suri = Pointer_stringify(uri);
      var element = document.createElement('a');
      element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(suri));
      element.setAttribute('download', sfilename);

      element.style.display = 'none';
      document.body.appendChild(element);

      element.click();

      document.body.removeChild(element);

  },

  BindWebGLTexture: function (texture) {
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
  },

});