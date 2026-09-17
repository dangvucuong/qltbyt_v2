import axiosClient from '@/api/axiosClient';

export async function openPrintHtml(path: string) {
  const res = await axiosClient.get(path, { responseType: 'text' });
  const html = typeof res.data === 'string' ? res.data : String(res.data);
  const win = window.open('', '_blank');
  if (!win) {
    throw new Error('Cannot open print window');
  }
  win.document.open();
  win.document.write(html);
  win.document.close();
}