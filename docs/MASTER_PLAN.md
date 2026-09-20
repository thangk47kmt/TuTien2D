# Tu Tien Du Hanh — Ke hoach tong quat (config + cong thuc + can bang)

Nguyen tac: giu repo hien tai. Khong rewrite. Moi so gameplay nam DB. Code chi tinh theo cong thuc. Admin sua so, khong viet cong thuc tu do.

Khong lam: GPS that, song tu online that, Phaser, eval string cong thuc.

---

## 1. Ba lop du lieu

1. GameRule — tham so toan cuc (xpBase, atkPerLevel, skillMul, ...).
2. Definition — nghe, do, quai, ruong, cong phap, ky nang nghe, loot, zone, nguong canh gioi.
3. Counter + BalanceBudget — he khac che va ngan sach chi so tung nghe.

Runtime: `ket_qua = f(rule, definition, player, context)`.
Admin chi sua hang DB. Client khong tu tinh so thuong.

---

## 2. Cong thuc (chi tiet)

Ky hieu: `R` = GameRule, `P` = Profession, `G` = do dang deo, `T` = cong phap active, `S` = ky nang nghe, `Z` = zone, `K` = he khac che.

### 2.1 Thuoc tinh nhan vat (tinh moi request)

```
atkBase = R.atkBase + level * R.atkPerLevel + RealmBonus(realm).atk
defBase = R.defBase + level * R.defPerLevel + RealmBonus.def
spiBase = R.spiBase + level * R.spiPerLevel + RealmBonus.spi
agiBase = R.agiBase + level * R.agiPerLevel + RealmBonus.agi
hpBase  = R.hpBase  + level * R.hpPerLevel  + RealmBonus.hp

atk = atkBase + P.AttackBonus + sum(G.atk * ProfessionGearMul) + T.atkFlat + floor(atk * T.AttackPercent/100)
def, spi, agi tuong tu
hp  = hpBase + sum(G.maxHp)
mp  = R.mpBase + spi * R.mpPerSpirit

ProfessionGearMul = 1 + G.ProfessionBonusPercent/100  neu dung nghe, else 1
```

RealmBonus lay tu bang `RealmThreshold` (khong if/else hardcode).

### 2.2 Tu vi (cultivation XP)

Len cap:

```
xpCan = R.xpBase + level * R.xpPerLevel + RealmBonus.xpExtra
```

Nhan tu vi khi settle phien / thang quai:

```
tuViPhien = phut * R.xpPerMinute
          * Z.AuraMultiplier
          * (1 + P.CultivationPercent/100)
          * (1 + T.CultivationPercent/100)
          * TravelBuff.Multiplier
          * Event.AuraMultiplier

tuViQuai  = Monster.CultivationXp
          * (1 + P.CultivationPercent/100)
          * RealmKillBonus
```

Idempotent: 1 session / 1 combat win chi 1 dong RewardTransaction.

### 2.3 Canh gioi

Bang `RealmThreshold`: Mortal, QiRefining, Foundation, ...

```
duocDoCanh = !player.RealmLocked
          && level >= row.minLevel
          && cultivationXpTong >= row.minLifetimeXp
          && (row.requiredTechniqueCode is null || da hoc)
```

Cuc Canh (LocksRealm=true) chan dong nay, doi ExtremePoints.

### 2.4 Luc chien (CombatPower)

Mot so de UI + tool can bang. Khong dung de tinh damage truc tiep.

```
CP = R.wAtk * atk + R.wDef * def + R.wSpi * spi + R.wAgi * agi
   + R.wHp * hp / 10 + R.wMp * mp / 10 + R.wSkill * SkillBudget(P)
```

Trong combat, damage van dung CombatMath + he khac che, khong dung CP.

### 2.5 Ky nang (cong phap + ky nang nghe)

Hai loai: TechniqueDefinition (hoc bang da, 1 active) va ProfessionSkill (mo khi chon nghe).

```
skillDmg = max(1, (atk * S.atkCoeff + spi * S.spiCoeff + R.skillFlat) * R.skillMul * (burst ? R.burstMul : 1) * CounterMul - enemyDef * R.defAbsorb)
selfDmg = S.selfDamageFlat + floor(hpMax * S.selfDamagePct/100)
heal    = S.healFlat + floor(spi * S.healCoeff)
```

Tag ky nang: Burst / Guard / Control / Sense / Craft / Fortune.

### 2.6 Combat thuong

```
playerHit = max(1, atk + rng(0,R.hitVariance) - enemyDef / R.defDivPlayer) * (skill ? R.skillMul : 1) * (burst ? R.burstMul : 1) * CounterMul
monsterHit = max(1, monAtk - playerDef / R.defDivMonster + rng(0,R.hitVariance)) * (defend ? R.defendMul : 1)
```

### 2.7 Ruong / loot

```
entryWeight hieu dung = entry.Weight * (1 + player.Fortune * R.fortuneLootPct / 100) * (pity >= table.PityThreshold ? R.pityBoost : 1)
```

---

## 3. He khac che nghe

10 nghe gom 5 truc (moi truc 2 nghe). Khac che vong.

| Truc | Nghe | The manh | Bi khac |
|---|---|---|---|
| Sat | An ninh, Ky thuat | Burst, pha giap | Phap ly khien, y te hoi |
| Thu | Phap ly, Y te | Giam sat, hoi | Nghe thuat / cong nghe neo |
| Van | Tai chinh, Nong | Da, tu vi, loot | Yeu burst thuan |
| Cam | Cong nghe, Giao duc | Sense, giam gia cong phap | Yeu HP/DEF |
| Hoa | Nghe thuat, Dich vu | Control, utility | Khong dam manh |

Bang ProfessionCounter: AttackerCode, DefenderCode, DamageMul, DefenseMul.
Mac dinh 1.00. Dieu kien: trung binh DamageMul moi nghe tan cong 9 nghe kia = 1.00 +/- 0.05.

---

## 4. Ngan sach thuoc tinh (auto balance)

Tai cung level, khong do, khong event:

```
RawScore(P) = wAtk*(atkBase+P.atk) + wDef*(defBase+P.def) + ... + wUnique * UniqueValue(P.skill)
TargetScore = mean(RawScore)
Band = TargetScore * R.balanceBand   (0.05)
```

UniqueValue: Tai chinh da%, Giao duc tu+discount, Cong nghe detect, Y te heal, An ninh burst coeff.

BalanceService.Evaluate / Suggest / Apply. Khong tu Apply luc start. Admin bam moi ghi + AuditLog.

---

## 5. Ky nang nghe

| Nghe | Ky nang | Tag | Hieu ung |
|---|---|---|---|
| Cong nghe | Quet Mach | Sense | +detect |
| Y te | Hoi Linh | Guard | Heal theo spi |
| Giao duc | Tam Truyen | Craft | Giam cost cong phap |
| Tai chinh | Tinh Van | Fortune | +% da thang / ruong |
| Phap ly | Khiem Chuong | Guard | Defend tot hon |
| Nghe thuat | Me Loan | Control | Giam atk quai 1 luot |
| Nong nghiep | Trach Linh | Fortune | +% tu vi zone Meadow |
| Dich vu | Thong Hanh | Utility | Giam MP sense |
| Ky thuat | Pha Giac | Burst | +dmg Elite/Boss |
| An ninh | Trap Kich | Burst | +dmg, mat it HP |

---

## 6. Admin tabs

GameRule, RealmThreshold, Profession+Skill, Counter 10x10, Item/Technique/Monster/Chest/Loot, Zone/Event, Balance Evaluate/Apply, Audit.

---

## 7. Lich

P0 Entity+seed so cu. P1 Gan calculator. P2 Ky nang+counter. P3 Admin CRUD UI. P4 Balance tool + test band 5%. P5 Chinh seed. Commit tung phase tren main.

## 8. Seed rule bang so hien tai

xpBase=60 xpPerLevel=35 xpPerMinute=3; atkBase=8 atkPerLevel=2; defBase=3 defPerLevel=1; spiBase=8 spiPerLevel=1; agiBase=5 agiPerLevel=1; hpBase=100 hpPerLevel=8; mpBase=60 mpPerSpirit=2; skillMul=1.55 burstMul=1.60 defendMul=0.45; qiRefining=5 foundation=12; balanceBand=0.05.

## 9. Xong khi

Sua so admin an combat lan sau. 10 nghe RawScore lech <= 5%. Moi nghe 1 ky nang. Khac che an damage. Profile co CombatPower. Khong hardcode nguong canh gioi.
