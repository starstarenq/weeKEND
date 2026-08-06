```ts
export interface Packet {
    clientId: string;
    type: "register" | "message";
    message: string;
}

export function encode(packet: Packet): string {
    return JSON.stringify(packet) + "\n";
}

export function decode(data: string): Packet {
    return JSON.parse(data);
}
```
