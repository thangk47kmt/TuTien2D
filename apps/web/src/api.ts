const API = import.meta.env.VITE_API_URL ?? '';
export type TokenResponse = { accessToken: string; refreshToken: string; expiresAtUtc: string; role: string };
export type Profession = { id: string; code: string; name: string; description: string; primaryBonusText: string; secondaryBonusText: string };
export type Profile = { id: string; name: string; professionCode: string; professionName: string; level: number; realm: string; realmStage: number; cultivationXp: number; xpRequired: number; hp: number; maxHp: number; mp: number; maxMp: number; attack: number; defense: number; spirit: number; agility: number; fortune: number; stability: number; spiritStones: number; professionPoints: number; extremePoints: number; realmLocked: boolean; mapX: number; mapY: number; activeTechniqueCode?: string; pityScore: number };
export type Entity = { kind: string; id: string; name: string; x: number; y: number; extra?: string };
export type World = { width: number; height: number; playerX: number; playerY: number; discovered: number[][]; entities: Entity[]; zoneName: string; aura: number };
export type Combat = { sessionId: string; monsterName: string; monsterHp: number; monsterMaxHp: number; playerHp: number; playerMaxHp: number; status: string; log: string[] };
export type Item = { id: string; name: string; icon: string; type: string; quality: string; quantity: number; equipped: boolean; locked: boolean; attack: number; defense: number; spirit: number; healAmount: number; preferredProfession?: string; professionBonusPercent: number; professionMatch: boolean };
export type Technique = { id: string; code: string; name: string; description: string; kind: string; learnCost: number; learned: boolean; active: boolean; locksRealm: boolean; warning?: string };
export type Rumor = { id: string; kind: string; title: string; body: string; approximateZone: string; reliability: number; expiresAtUtc: string };
export type TravelLoc = { id: string; code: string; name: string; distanceKm: number };
function key(name: string) { return `tutien.${name}`; }
export function getAccess() { return localStorage.getItem(key('access')); }
export function getRefresh() { return localStorage.getItem(key('refresh')); }
export function getRole() { return localStorage.getItem(key('role')); }
export function saveTokens(t: TokenResponse) {
  localStorage.setItem(key('access'), t.accessToken);
  localStorage.setItem(key('refresh'), t.refreshToken);
  localStorage.setItem(key('role'), t.role);
}
export function clearTokens() {
  localStorage.removeItem(key('access'));
  localStorage.removeItem(key('refresh'));
  localStorage.removeItem(key('role'));
}
async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  headers.set('Content-Type', 'application/json');
  const access = getAccess();
  if (access) headers.set('Authorization', `Bearer ${access}`);
  const res = await fetch(`${API}${path}`, { ...init, headers });
  if (res.status === 401 && getRefresh()) {
    const refreshed = await fetch(`${API}/api/v1/auth/refresh`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ refreshToken: getRefresh() }) });
    if (refreshed.ok) { saveTokens(await refreshed.json()); return request<T>(path, init); }
    clearTokens();
  }
  if (!res.ok) {
    let msg = res.statusText;
    try { const err = await res.json(); msg = err.message ?? err.Message ?? msg; } catch {}
    throw new Error(msg);
  }
  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
export const api = {
  register: (userName: string, email: string, password: string) => request<TokenResponse>('/api/v1/auth/register', { method: 'POST', body: JSON.stringify({ userName, email, password }) }),
  login: (userName: string, password: string) => request<TokenResponse>('/api/v1/auth/login', { method: 'POST', body: JSON.stringify({ userName, password }) }),
  me: () => request<{ id: string; userName: string; role: string }>('/api/v1/auth/me'),
  professions: () => request<Profession[]>('/api/v1/professions'),
  createPlayer: (name: string, professionCode: string) => request<Profile>('/api/v1/players', { method: 'POST', body: JSON.stringify({ name, professionCode, idempotencyKey: crypto.randomUUID() }) }),
  profile: () => request<Profile>('/api/v1/players/me'),
  world: () => request<World>('/api/v1/world'),
  move: (x: number, y: number) => request<World>('/api/v1/world/move', { method: 'POST', body: JSON.stringify({ x, y }) }),
  sense: () => request<World>('/api/v1/world/sense', { method: 'POST' }),
  startCombat: (spawnId: string) => request<Combat>('/api/v1/combat/start', { method: 'POST', body: JSON.stringify({ spawnId, idempotencyKey: crypto.randomUUID() }) }),
  act: (id: string, action: number, itemId?: string) => request<Combat>(`/api/v1/combat/${id}/action`, { method: 'POST', body: JSON.stringify({ action, itemId, idempotencyKey: crypto.randomUUID() }) }),
  inventory: () => request<Item[]>('/api/v1/inventory'),
  equip: (itemId: string) => request<Item[]>('/api/v1/inventory/equip', { method: 'POST', body: JSON.stringify({ itemId }) }),
  unequip: (itemId: string) => request<Item[]>('/api/v1/inventory/unequip', { method: 'POST', body: JSON.stringify({ itemId }) }),
  useItem: (id: string) => request<void>(`/api/v1/inventory/${id}/use`, { method: 'POST' }),
  techniques: () => request<Technique[]>('/api/v1/techniques'),
  learn: (code: string) => request<void>('/api/v1/techniques/learn', { method: 'POST', body: JSON.stringify({ code }) }),
  openChest: (chestId: string) => request<{ type: string; text: string; quantity: number }[]>('/api/v1/chests/open', { method: 'POST', body: JSON.stringify({ chestId, idempotencyKey: crypto.randomUUID() }) }),
  startCultivate: (minutes: number) => request<{ sessionId: string; status: string; endUtc: string; expectedXp: number; canSettle: boolean }>('/api/v1/cultivation/start', { method: 'POST', body: JSON.stringify({ minutes }) }),
  cultivate: () => request<{ sessionId?: string; status: string; endUtc?: string; expectedXp?: number; canSettle: boolean }>('/api/v1/cultivation'),
  settle: () => request<{ type: string; text: string; quantity: number }[]>('/api/v1/cultivation/settle', { method: 'POST', body: JSON.stringify({}) }),
  rumors: () => request<Rumor[]>('/api/v1/rumors'),
  locations: () => request<TravelLoc[]>('/api/v1/travel/locations'),
  checkIn: (locationId: string) => request<void>('/api/v1/travel/check-in', { method: 'POST', body: JSON.stringify({ locationId, idempotencyKey: crypto.randomUUID() }) }),
  buff: () => request<{ multiplier: number; expiresAtUtc: string; sourceLocation: string } | null>('/api/v1/travel/buff'),
  enterRealm: () => request<{ name: string; endUtc: string; message: string }>('/api/v1/secret-realms/enter', { method: 'POST' }),
  admin: () => request<Record<string, unknown>>('/api/v1/admin/overview'),
  setWeight: (id: string, spawnWeight: number) => request<void>(`/api/v1/admin/monsters/${id}/weight`, { method: 'PUT', body: JSON.stringify({ spawnWeight }) }),
  setAura: (id: string, multiplier: number) => request<void>(`/api/v1/admin/zones/${id}/aura`, { method: 'PUT', body: JSON.stringify({ multiplier }) }),
  toggleEvent: (id: string, enabled: boolean) => request<void>(`/api/v1/admin/events/${id}`, { method: 'PUT', body: JSON.stringify({ enabled }) }),
  rewards: () => request<unknown[]>('/api/v1/admin/rewards?take=30'),
  combats: () => request<unknown[]>('/api/v1/admin/combats?take=20'),
};
