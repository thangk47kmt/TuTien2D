import { useEffect, useState } from 'react'
import { api, clearTokens, getAccess, getRole, saveTokens, type Combat, type Item, type Profession, type Profile, type World } from './api'

export default function App() {
  const [ready, setReady] = useState(false)
  const [authed, setAuthed] = useState(!!getAccess())
  const [profile, setProfile] = useState<Profile | null>(null)
  const [world, setWorld] = useState<World | null>(null)
  const [combat, setCombat] = useState<Combat | null>(null)
  const [items, setItems] = useState<Item[]>([])
  const [err, setErr] = useState('')
  const [user, setUser] = useState('')
  const [pass, setPass] = useState('Play#123')
  const [name, setName] = useState('Dao Huu')
  const [professions, setProfessions] = useState<Profession[]>([])
  const [code, setCode] = useState('technology')

  useEffect(() => {
    (async () => {
      if (!getAccess()) { setReady(true); return }
      try {
        await api.me()
        try { setProfile(await api.profile()); setWorld(await api.world()) } catch {}
        setAuthed(true)
      } catch { clearTokens(); setAuthed(false) }
      setReady(true)
    })()
  }, [])

  async function login() {
    try {
      saveTokens(await api.login(user || 'daoist', pass))
      setAuthed(true)
      try { setProfile(await api.profile()); setWorld(await api.world()) } catch { setProfessions(await api.professions()) }
    } catch (e) { setErr(e instanceof Error ? e.message : 'Loi dang nhap') }
  }

  async function create() {
    try {
      setProfile(await api.createPlayer(name, code))
      setWorld(await api.world())
    } catch (e) { setErr(e instanceof Error ? e.message : 'Loi tao nhan vat') }
  }

  async function step(dx: number, dy: number) {
    if (!world) return
    try {
      const next = await api.move(world.playerX + dx, world.playerY + dy)
      setWorld(next)
      const mon = next.entities.find(e => e.kind === 'monster' && Math.abs(e.x - next.playerX) + Math.abs(e.y - next.playerY) <= 1)
      if (mon) setCombat(await api.startCombat(mon.id))
    } catch (e) { setErr(e instanceof Error ? e.message : 'Loi di chuyen') }
  }

  if (!ready) return <div className="boot">Dang mo tien lo</div>
  if (!authed) return (
    <div className="gate">
      <div className="seal">仙</div>
      <h1>Tu Tien Du Hanh</h1>
      <label>Tai khoan<input value={user} onChange={e => setUser(e.target.value)} placeholder="daoist" /></label>
      <label>Mat khau<input value={pass} onChange={e => setPass(e.target.value)} type="password" /></label>
      {err && <p className="banner">{err}</p>}
      <button onClick={() => void login()}>Dang nhap</button>
    </div>
  )
  if (!profile) return (
    <div className="gate">
      <h2>Tao nhan vat</h2>
      <label>Ten<input value={name} onChange={e => setName(e.target.value)} /></label>
      <label>Nghe<select value={code} onChange={e => setCode(e.target.value)}>{professions.map(p => <option key={p.code} value={p.code}>{p.name}</option>)}</select></label>
      {err && <p className="banner">{err}</p>}
      <button onClick={() => void create()}>Nhap the</button>
    </div>
  )

  return (
    <div className="shell">
      <header className="topbar"><div className="brand"><span>仙</span> Tu Tien Du Hanh</div><button className="ghost" onClick={() => { clearTokens(); setAuthed(false) }}>Thoat</button></header>
      <aside className="sidebar">
        <div className="who"><b>{profile.name}</b><small>{profile.realm} Lv {profile.level}</small></div>
        <button onClick={async () => setItems(await api.inventory())}>Tui do</button>
        {getRole() === 'Admin' && <button onClick={async () => setErr(JSON.stringify(await api.admin()))}>Admin</button>}
      </aside>
      <main className="main">
        {err && <p className="banner">{err}</p>}
        <div className="hud">
          <div>HP {profile.hp}/{profile.maxHp}<div className="bar"><span className="hp" style={{ width: `${profile.hp / profile.maxHp * 100}%` }} /></div></div>
          <div>MP {profile.mp}/{profile.maxMp}<div className="bar"><span className="mp" style={{ width: `${profile.mp / profile.maxMp * 100}%` }} /></div></div>
          <div>XP {profile.cultivationXp}/{profile.xpRequired}<div className="bar"><span className="xp" style={{ width: `${profile.cultivationXp / profile.xpRequired * 100}%` }} /></div></div>
          <div className="gold">{profile.spiritStones} linh thach</div>
        </div>
        {world && (
          <>
            <div className="map-head"><div>{world.zoneName}<small>Aura x{world.aura} · ({world.playerX},{world.playerY})</small></div></div>
            <div className="map" style={{ gridTemplateColumns: `repeat(${world.width}, var(--tile))` }}>
              {Array.from({ length: world.height * world.width }, (_, i) => {
                const x = i % world.width, y = Math.floor(i / world.width)
                const seen = world.discovered.some(c => c[0] === x && c[1] === y)
                const here = world.entities.find(e => e.x === x && e.y === y)
                const me = world.playerX === x && world.playerY === y
                return <div key={i} className={`tile ${me ? 'player' : ''} ${seen ? '' : 'fog'}`}>{me ? '人' : here?.kind === 'monster' ? '兽' : here?.kind === 'chest' ? '匣' : ''}</div>
              })}
            </div>
            <div className="pad">
              <div><button onClick={() => void step(0, -1)}>Len</button></div>
              <div><button onClick={() => void step(-1, 0)}>Trai</button><button onClick={() => void step(1, 0)}>Phai</button></div>
              <div><button onClick={() => void step(0, 1)}>Xuong</button></div>
            </div>
          </>
        )}
        {combat && (
          <dialog open className="modal">
            <h3>{combat.monsterName}</h3>
            <p>Quai {combat.monsterHp}/{combat.monsterMaxHp} · Ban {combat.playerHp}</p>
            {combat.log.slice(-4).map((l, i) => <p key={i}>{l}</p>)}
            {combat.status === 'Active' ? (
              <div className="grid2">
                <button onClick={async () => setCombat(await api.act(combat.sessionId, 0))}>Danh</button>
                <button onClick={async () => setCombat(await api.act(combat.sessionId, 4))}>Rut</button>
              </div>
            ) : <button onClick={async () => { setCombat(null); setProfile(await api.profile()); setWorld(await api.world()) }}>Dong</button>}
          </dialog>
        )}
        {items.length > 0 && <section className="cards">{items.map(it => <article className="card" key={it.id}><h3>{it.icon} {it.name}</h3><p>{it.quantity} · {it.quality}</p>{it.equipped ? <button onClick={async () => setItems(await api.unequip(it.id))}>Thao</button> : <button onClick={async () => setItems(await api.equip(it.id))}>Trang bi</button>}</article>)}</section>}
      </main>
    </div>
  )
}
