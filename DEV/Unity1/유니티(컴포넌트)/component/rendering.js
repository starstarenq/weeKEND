(() => {
  "use strict";
  const components = window.GameComponents = window.GameComponents || {};

  function roundedRect(ctx, x, y, width, height, radius) {
    ctx.beginPath();
    ctx.roundRect(x, y, width, height, radius);
  }

  function drawArena(ctx, width, height) {
    ctx.clearRect(0, 0, width, height);
    const gradient = ctx.createRadialGradient(width * .5, height * .45, 20, width * .5, height * .45, width * .7);
    gradient.addColorStop(0, "#14233a");
    gradient.addColorStop(1, "#0a111e");
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, width, height);

    ctx.strokeStyle = "rgba(113, 148, 199, .08)";
    ctx.lineWidth = 1;
    for (let x = 24; x < width; x += 32) {
      ctx.beginPath(); ctx.moveTo(x, 0); ctx.lineTo(x, height); ctx.stroke();
    }
    for (let y = 20; y < height; y += 32) {
      ctx.beginPath(); ctx.moveTo(0, y); ctx.lineTo(width, y); ctx.stroke();
    }
    const vignette = ctx.createRadialGradient(width / 2, height / 2, height * .2, width / 2, height / 2, height * .75);
    vignette.addColorStop(0, "transparent");
    vignette.addColorStop(1, "rgba(0, 0, 0, .42)");
    ctx.fillStyle = vignette;
    ctx.fillRect(0, 0, width, height);
  }

  function drawEntity(ctx, entity, options) {
    const { color, darkColor, label, eyeColor = "#fff" } = options;
    ctx.save();
    ctx.translate(entity.x, entity.y + entity.bobOffset);
    ctx.rotate(entity.rotation);
    ctx.shadowColor = color;
    ctx.shadowBlur = entity.flash > 0 ? 28 : 14;
    const size = entity.radius * 2;
    roundedRect(ctx, -entity.radius, -entity.radius, size, size, entity.radius * .42);
    const gradient = ctx.createLinearGradient(-entity.radius, -entity.radius, entity.radius, entity.radius);
    gradient.addColorStop(0, color);
    gradient.addColorStop(1, darkColor);
    ctx.fillStyle = gradient;
    ctx.fill();
    ctx.shadowBlur = 0;
    ctx.fillStyle = eyeColor;
    ctx.beginPath();
    ctx.arc(entity.radius * .22, -entity.radius * .16, 3, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    ctx.save();
    ctx.font = "600 10px system-ui";
    ctx.textAlign = "center";
    ctx.fillStyle = "rgba(220, 232, 248, .72)";
    ctx.fillText(label, entity.x, entity.y + entity.radius + 20);
    ctx.restore();
  }

  function drawTrail(ctx, trail, color) {
    trail.forEach((point, index) => {
      const alpha = (index + 1) / trail.length * .16;
      ctx.fillStyle = color.replace(")", `, ${alpha})`).replace("rgb(", "rgba(");
      if (color.startsWith("#")) {
        ctx.globalAlpha = alpha;
        ctx.fillStyle = color;
      }
      ctx.beginPath();
      ctx.arc(point.x, point.y, point.radius * (index + 1) / trail.length, 0, Math.PI * 2);
      ctx.fill();
    });
    ctx.globalAlpha = 1;
  }

  function drawPulse(ctx, x, y, strength) {
    ctx.save();
    ctx.globalAlpha = strength;
    ctx.strokeStyle = "#f6c85f";
    ctx.lineWidth = 3;
    ctx.beginPath();
    ctx.arc(x, y, 28 + (1 - strength) * 42, 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();
  }

  components.rendering = { drawArena, drawEntity, drawTrail, drawPulse };
})();
