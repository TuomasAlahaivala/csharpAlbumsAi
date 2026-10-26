export function StatusMessage({ message }: { message: string }) {
  return <div className="status-message" role="alert">{message}</div>;
}
