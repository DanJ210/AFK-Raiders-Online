export const liveClosedMessage = 'Live connection closed. Snapshot polling continues; your Raider still runs on the server.'

export function clearLiveWarning(message: string): string {
  return message === liveClosedMessage ? '' : message
}

export async function stopOwnedConnection<T extends { stop(): Promise<void> }>(
  current: T | null,
  release: () => void,
): Promise<void> {
  release()
  if (current) await current.stop()
}
