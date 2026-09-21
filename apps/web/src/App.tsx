import { useEffect, useState } from 'react'
import { api, clearTokens, combatPower, getAccess, getRole, saveTokens, type AdminMon, type AdminProf, type Balance, type Catalog, type Combat, type Item, type Profession, type Profile, type Rumor, type Technique, type TravelLoc, type World } from './api'

type Panel = 'map' | 'bag' | 'tech' | 'cult' | 'rumor' | 'travel' | 'admin'

export default function App() {
  const [ready, setReady] = useState(false)
  const [authed, setAuthed] = useState(!!getAccess())
  const [profile, setProfile] = useState<Profile | null>(null)
  const [world, setWorld] = useState<World | null>(null)
  const [combat, setCombat] = useState<Combat | null>(null)
  const [items, setItems] = useState<Item[]>([])
  const [techs, setTechs] = useState<Technique[]>([])
  const [rumors, setRumors] = useState<Rumor[]>([])
  const [locs, setLocs] = useState<TravelLoc[]>([])
  const [cult, setCult] = useState<{ status: string; endUtc?: string; expectedXp?: number; canSettle: boolean } | null>(null)
  const [catalog, setCatalog] = useState<Catalog | null>(null)
  const [balance, setBalance] = useState<Balance | null>(null)
  const [panel, setPanel] = useState<Panel>('map')
  const [err, setErr] = useState('')
  const [user, setUser] = useState('')
  const [pass, setPass] = useState('Play#123')
  const [name, setName] = useState('Dao Huu')
  const [professions, setProfessions] = useState<Profession[]>([])
  const [code, setCode] = useState('technology')

  useEffect(() => { (async () => {
    if (!getAccess()) { setReady(true); return }
    try { await api.me(); try { setProfile(await api.profile()); setWorld(await api.world()) } catch {}; setAuthed(true) }
    catch { clearTokens(); setAuthed(false) }
    setReady(true)
  })() }, [])

  async function login() {
    try { saveTokens(await api.login(user || 'daoist', pass)); setAuthed(true); try { setProfile(await api.profile()); setWorld(await api.world()) } catch { setProfessions(await api.professions()) } }
    catch (e) { setErr(e instanceof Error ? e.message : 'Loi dang nhap') }
  }
  async function create() {
    try { setProfile(await api.createPlayer(name, code)); setWorld(await api.world()) }
    catch (e) { setErr(e instanceof Error ? e.message : 'Loi tao nhan vat') }
  }
  async function step(dx: number, dy: number) {
    if (!world) return
    try {
      const next = await api.move(world.playerX + dx, world.playerY + dy)
      setWorld(next)
      const near = next.entities.filter(e => Math.abs(e.x - next.playerX) + Math.abs(e.y - next.playerY) <= 1)
      const mon = near.find(e => e.kind === 'monster')
      const chest = near.find(e => e.kind === 'chest')
      if (mon) setCombat(await api.startCombat(mon.id))
      else if (chest) { const loot = await api.openChest(chest.id); setErr(loot.map(l => l.text).join(' · ')); setProfile(await api.profile()); setWorld(await api.world()) }
    } catch (e) { setErr(e instanceof Error ? e.message : 'Loi di chuyen') }
  }
  async function open(p: Panel) {
    setPanel(p)
    try {
      if (p === 'bag') setItems(await api.inventory())
      if (p === 'tech') setTechs(await api.techniques())
      if (p === 'rumor') setRumors(await api.rumors())
      if (p === 'travel') setLocs(await api.locations())
      if (p === 'cult') setCult(await api.cultivate())
      if (p === 'admin') { setCatalog(await api.catalog()); setBalance(await api.balance(5)) }
    } catch (e) { setErr(e instanceof Error ? e.message : 'Loi') }
  }

  if (!ready) return <div className="boot">Dang mo tien lo</div>
  if (!authed) return (<div className="gate"><div className="seal">仙</div><h1>Tu Tien Du Hanh</h1><label>Tai khoan<input value={user} onChange={e => setUser(e.target.value)} placeholder="daoist" /></label><label>Mat khau<input value={pass} onChange={e => setPass(e.target.value)} type="password" /></label>{err && <p className="banner">{err}</p>}<button onClick={() => void login()}>Dang nhap</button></div>)
  if (!profile) return (<div className="gate"><h2>Tao nhan vat</h2><label>Ten<input value={name} onChange={e => setName(e.target.value)} /></label><label>Nghe<select value={code} onChange={e => setCode(e.target.value)}>{professions.map(p => <option key={p.code} value={p.code}>{p.name}</option>)}</select></label>{err && <p className="banner">{err}</p>}<button onClick={() => void create()}>Nhap the</button></div>)

  const cp = combatPower(profile)

  return (
    <div className="shell">
      <header className="topbar"><div className="brand"><span>仙</span> Tu Tien Du Hanh</div><button className="ghost" onClick={() => { clearTokens(); setAuthed(false) }}>Thoat</button></header>
      <aside className="sidebar">
        <div className="who"><b>{profile.name}</b><small>{profile.realm} Lv {profile.level}{profile.realmLocked ? ' · khoa' : ''}</small></div>
        {(['map','bag','tech','cult','rumor','travel'] as Panel[]).map(p => <button key={p} className={panel===p?'active':''} onClick={() => void open(p)}>{p}</button>)}
        {getRole() === 'Admin' && <button onClick={() => void open('admin')}>admin</button>}
      </aside>
      <main className="main">
        {err && <p className="banner">{err}</p>}
        <div className="hud">
          <div>HP {profile.hp}/{profile.maxHp}<div className="bar"><span className="hp" style={{ width: `${profile.hp / Math.max(1,profile.maxHp) * 100}%` }} /></div></div>
          <div>MP {profile.mp}/{profile.maxMp}<div className="bar"><span className="mp" style={{ width: `${profile.mp / Math.max(1,profile.maxMp) * 100}%` }} /></div></div>
          <div>XP {profile.cultivationXp}/{profile.xpRequired}<div className="bar"><span className="xp" style={{ width: `${profile.cultivationXp / Math.max(1,profile.xpRequired) * 100}%` }} /></div></div>
          <div className="gold">{profile.spiritStones} da · LC {cp} · {profile.attack}/{profile.defense}/{profile.spirit}</div>
        </div>
        {panel === 'map' && world && (<>
          <div className="map-head"><div>{world.zoneName}<small>Aura x{world.aura} · ({world.playerX},{world.playerY})</small></div><button onClick={async () => setWorld(await api.sense())}>Cam ung</button></div>
          <div className="map" style={{ gridTemplateColumns: `repeat(${world.width}, var(--tile))` }}>
            {Array.from({ length: world.height * world.width }, (_, i) => {
              const x = i % world.width, y = Math.floor(i / world.width)
              const seen = world.discovered.some(c => c[0] === x && c[1] === y)
              const here = world.entities.find(e => e.x === x && e.y === y)
              const me = world.playerX === x && world.playerY === y
              return <div key={i} className={`tile ${me ? 'player' : ''} ${seen ? '' : 'fog'}`}>{me ? '人' : here?.kind === 'monster' ? '兽' : here?.kind === 'chest' ? '匣' : ''}</div>
            })}
          </div>
          <div className="pad"><div><button onClick={() => void step(0,-1)}>Len</button></div><div><button onClick={() => void step(-1,0)}>Trai</button><button onClick={() => void step(1,0)}>Phai</button></div><div><button onClick={() => void step(0,1)}>Xuong</button></div></div>
        </>)}
        {panel === 'bag' && <section className="cards">{items.map(it => <article className="card" key={it.id}><h3>{it.icon} {it.name}</h3><p>{it.quantity} · {it.quality}{it.professionMatch ? ' · dung nghe' : ''}</p>{it.healAmount>0 && <button onClick={async () => { await api.useItem(it.id); setItems(await api.inventory()); setProfile(await api.profile()) }}>Dung</button>}{it.equipped ? <button onClick={async () => setItems(await api.unequip(it.id))}>Thao</button> : <button onClick={async () => { try { setItems(await api.equip(it.id)); setProfile(await api.profile()) } catch(e){ setErr(e instanceof Error?e.message:'Loi') } }}>Trang bi</button>}</article>)}</section>}
        {panel === 'tech' && <section className="cards">{techs.map(t => <article className="card" key={t.id}><h3>{t.name}</h3><p>{t.description}</p>{t.warning && <p className="warn">{t.warning}</p>}<p>{t.learned ? (t.active ? 'Dang van' : 'Da hoc') : `${t.learnCost} da`}</p><button onClick={async () => { await api.learn(t.code); setTechs(await api.techniques()); setProfile(await api.profile()) }}>{t.learned ? 'Kich hoat' : 'Hoc'}</button></article>)}</section>}
        {panel === 'cult' && <section className="card"><h3>Tu luyen</h3><p>{cult?.status} {cult?.endUtc && `den ${cult.endUtc}`}</p><p>Du kien {cult?.expectedXp ?? 0} tu vi</p><button onClick={async () => { setCult(await api.startCultivate(5)); setErr('Da ngoi 5 phut server.') }}>Ngoi 5 phut</button>{cult?.canSettle && <button onClick={async () => { const r = await api.settle(); setErr(r.map(x=>x.text).join(' · ')); setCult(await api.cultivate()); setProfile(await api.profile()) }}>Ket toan</button>}</section>}
        {panel === 'rumor' && <section className="cards">{rumors.map(r => <article className="card" key={r.id}><h3>{r.title}</h3><p>{r.body}</p><small>{r.approximateZone} · {r.reliability}%</small></article>)}</section>}
        {panel === 'travel' && <section className="cards">{locs.map(l => <article className="card" key={l.id}><h3>{l.name}</h3><p>{l.distanceKm} km (mock)</p><button onClick={async () => { await api.checkIn(l.id); setErr('Check-in gia lap xong') }}>Check-in</button></article>)}</section>}
        {panel === 'admin' && <AdminDesk catalog={catalog} balance={balance} onReload={() => void open('admin')} onErr={setErr} />}
        {combat && <dialog open className="modal"><h3>{combat.monsterName}</h3><p>Quai {combat.monsterHp}/{combat.monsterMaxHp} · Ban {combat.playerHp}</p>{combat.log.slice(-5).map((l,i)=><p key={i}>{l}</p>)}{combat.status==='Active' ? <div className="grid2"><button onClick={async () => setCombat(await api.act(combat.sessionId, 0))}>Danh</button><button onClick={async () => setCombat(await api.act(combat.sessionId, 1))}>Van cong</button><button onClick={async () => setCombat(await api.act(combat.sessionId, 2))}>Thu</button><button onClick={async () => setCombat(await api.act(combat.sessionId, 4))}>Rut</button></div> : <button onClick={async () => { setCombat(null); setProfile(await api.profile()); setWorld(await api.world()); setItems(await api.inventory()) }}>Dong</button>}</dialog>}
      </main>
    </div>
  )
}

function AdminDesk({ catalog, balance, onReload, onErr }: { catalog: Catalog | null; balance: Balance | null; onReload: () => void; onErr: (s: string) => void }) {
  const [profs, setProfs] = useState<AdminProf[]>(catalog?.professions ?? [])
  const [mons, setMons] = useState<AdminMon[]>(catalog?.monsters ?? [])
  useEffect(() => { setProfs(catalog?.professions ?? []); setMons(catalog?.monsters ?? []) }, [catalog])
  return (
    <div>
      <h3>Admin</h3>
      <p>Sua so roi luu. Balance level 5, band {balance?.band} target {balance?.target}.</p>
      <button onClick={async () => { await api.applyBalance(5); onErr('Da apply balance'); onReload() }}>Apply can bang</button>
      {balance && <section className="cards">{balance.rows.map(r => <article className="card" key={r.code}><h3>{r.name}</h3><p>ATK {r.attack} DEF {r.defense} SPI {r.spirit}</p><p>Raw {r.raw} {balance.flags.find(f => f.code===r.code)?.ok ? 'OK' : 'LECH'}</p></article>)}</section>}
      <h4>Nghe</h4>
      {profs.map((p, i) => <div className="card" key={p.id}>
        <b>{p.name}</b>
        {(['attackBonus','defenseBonus','spiritBonus','cultivationPercent'] as const).map(k => (
          <label key={k}>{k}<input type="number" value={p[k]} onChange={e => { const n=[...profs]; n[i] = { ...p, [k]: Number(e.target.value) }; setProfs(n) }} /></label>
        ))}
        <button onClick={async () => { await api.saveProfession(p); onErr('Luu nghe ' + p.code); onReload() }}>Luu</button>
      </div>)}
      <h4>Quai</h4>
      {mons.map((m, i) => <div className="card" key={m.id}>
        <b>{m.name}</b>
        {(['hp','attack','defense','spawnWeight'] as const).map(k => (
          <label key={k}>{k}<input type="number" value={m[k]} onChange={e => { const n=[...mons]; n[i] = { ...m, [k]: Number(e.target.value) }; setMons(n) }} /></label>
        ))}
        <button onClick={async () => { await api.saveMonster(m); onErr('Luu quai ' + m.code); onReload() }}>Luu</button>
      </div>)}
      {catalog?.skills && <p>Ky nang nghe: {catalog.skills.map(s => s.name).join(', ')}</p>}
      {catalog?.counters && <p>Khac che: {catalog.counters.map(c => `${c.attackerCode}->${c.defenderCode} x${c.damageMul}`).join(' | ')}</p>}
    </div>
  )
}
