// TGA(무압축, 24/32bpp truecolor 또는 8bpp grayscale) 파서 + WebGL 텍스처 로더.
// 픽셀 데이터는 BGR(A) 순서로 저장되어 있으므로 RGBA로 스위즐하고,
// 이미지 디스크립터의 origin 비트를 읽어 WebGL 업로드 시 뒤집을지 여부를 판단한다.
window.TGALoader = (function () {
  function parseTGA(arrayBuffer) {
    const data = new Uint8Array(arrayBuffer);
    const idLength = data[0];
    const colorMapType = data[1];
    const imageType = data[2];
    const width = data[12] | (data[13] << 8);
    const height = data[14] | (data[15] << 8);
    const bpp = data[16];
    const descriptor = data[17];

    if (colorMapType !== 0) {
      throw new Error('컬러맵 TGA는 지원하지 않습니다.');
    }
    if (imageType !== 2 && imageType !== 3) {
      throw new Error('압축(RLE) TGA는 지원하지 않습니다. imageType=' + imageType);
    }
    if (bpp !== 32 && bpp !== 24 && bpp !== 8) {
      throw new Error('지원하지 않는 bpp: ' + bpp);
    }

    const offset = 18 + idLength;
    const bytesPerPixel = bpp / 8;
    const pixelCount = width * height;
    const rgba = new Uint8Array(pixelCount * 4);

    for (let i = 0; i < pixelCount; i++) {
      const src = offset + i * bytesPerPixel;
      const dst = i * 4;
      if (bpp === 32) {
        rgba[dst] = data[src + 2];
        rgba[dst + 1] = data[src + 1];
        rgba[dst + 2] = data[src];
        rgba[dst + 3] = data[src + 3];
      } else if (bpp === 24) {
        rgba[dst] = data[src + 2];
        rgba[dst + 1] = data[src + 1];
        rgba[dst + 2] = data[src];
        rgba[dst + 3] = 255;
      } else {
        const v = data[src];
        rgba[dst] = v;
        rgba[dst + 1] = v;
        rgba[dst + 2] = v;
        rgba[dst + 3] = 255;
      }
    }

    const topOrigin = (descriptor & 0x20) !== 0;
    return { width: width, height: height, pixels: rgba, flipY: topOrigin };
  }

  function base64ToArrayBuffer(base64) {
    const binaryString = atob(base64);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
  }

  function loadTGATextureFromBase64(gl, base64) {
    const tga = parseTGA(base64ToArrayBuffer(base64));
    const texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, tga.flipY);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, tga.width, tga.height, 0, gl.RGBA, gl.UNSIGNED_BYTE, tga.pixels);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.REPEAT);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.bindTexture(gl.TEXTURE_2D, null);
    return { texture: texture, width: tga.width, height: tga.height };
  }

  return {
    parseTGA: parseTGA,
    base64ToArrayBuffer: base64ToArrayBuffer,
    loadTGATextureFromBase64: loadTGATextureFromBase64,
  };
})();
