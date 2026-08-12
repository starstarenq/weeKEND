(() => {
  "use strict";
  const components = window.GameComponents = window.GameComponents || {};

  function clampToBounds(body, bounds) {
    const minX = body.radius;
    const maxX = Math.max(minX, bounds.width - body.radius);
    const minY = body.radius;
    const maxY = Math.max(minY, bounds.height - body.radius);
    if (body.x < minX || body.x > maxX) body.vx *= -body.restitution;
    if (body.y < minY || body.y > maxY) body.vy *= -body.restitution;
    body.x = Math.max(minX, Math.min(maxX, body.x));
    body.y = Math.max(minY, Math.min(maxY, body.y));
  }

  function resolveCircleCollision(a, b, restitution = .8) {
    const dx = b.x - a.x;
    const dy = b.y - a.y;
    const minDistance = a.radius + b.radius;
    const distanceSquared = dx * dx + dy * dy;
    if (distanceSquared >= minDistance * minDistance) {
      return { collided: false, normal: { x: 0, y: 0 } };
    }

    const distance = Math.sqrt(distanceSquared) || .001;
    const normal = { x: dx / distance, y: dy / distance };
    const overlap = minDistance - distance;
    const totalInverseMass = 1 / a.mass + 1 / b.mass;
    a.x -= normal.x * overlap * (1 / a.mass) / totalInverseMass;
    a.y -= normal.y * overlap * (1 / a.mass) / totalInverseMass;
    b.x += normal.x * overlap * (1 / b.mass) / totalInverseMass;
    b.y += normal.y * overlap * (1 / b.mass) / totalInverseMass;

    const relativeVelocity = (b.vx - a.vx) * normal.x + (b.vy - a.vy) * normal.y;
    if (relativeVelocity < 0) {
      const impulse = -(1 + restitution) * relativeVelocity / totalInverseMass;
      a.vx -= impulse * normal.x / a.mass;
      a.vy -= impulse * normal.y / a.mass;
      b.vx += impulse * normal.x / b.mass;
      b.vy += impulse * normal.y / b.mass;
    }
    return { collided: true, normal };
  }

  components.collision = { clampToBounds, resolveCircleCollision };
})();
