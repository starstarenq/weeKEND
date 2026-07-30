(() => {
  "use strict";
  const components = window.GameComponents = window.GameComponents || {};

  class GameLoop {
    constructor(update, render, onTelemetry = () => {}) {
      this.update = update;
      this.render = render;
      this.onTelemetry = onTelemetry;
      this.running = false;
      this.lastTime = 0;
      this.fps = 60;
      this.frame = this.frame.bind(this);
    }

    start() {
      if (this.running) return;
      this.running = true;
      this.lastTime = performance.now();
      requestAnimationFrame(this.frame);
    }

    stop() {
      this.running = false;
    }

    frame(now) {
      if (!this.running) return;
      const dt = Math.min((now - this.lastTime) / 1000, .05);
      this.lastTime = now;
      this.fps += ((dt > 0 ? 1 / dt : 60) - this.fps) * .08;
      this.update(dt);
      this.render();
      this.onTelemetry({ fps: this.fps, dt });
      requestAnimationFrame(this.frame);
    }
  }

  components.GameLoop = GameLoop;
})();
