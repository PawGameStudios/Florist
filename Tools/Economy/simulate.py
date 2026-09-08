"""Config-driven balance simulation; no Unity, network or third-party Python packages.

Run Export-Configs.ps1 first. This models correct receipts and chosen service rates,
not input skill, animations, session retention or actual ad revenue.
"""
import argparse
import json
import math
import os
from pathlib import Path
import random
import statistics

parser = argparse.ArgumentParser()
parser.add_argument('--export-dir', type=Path, default=Path(os.environ['TEMP']) / 'florist-economy')
parser.add_argument('--report', type=Path, default=Path('Docs/EconomySimulation.md'))
args = parser.parse_args()

def config(name):
    return json.loads((args.export_dir / (name + '.json')).read_text(encoding='utf-8-sig'))['MonoBehaviour']

shop, workshop, level, customers, economy = [config(n) for n in
    ('ShopConfig', 'WorkshopConfig', 'LevelConfig', 'CustomerConfig', 'EconomyConfig')]
groups = {name: shop[name] for name in ('FlowerItems', 'WrapperItems', 'RibbonItems', 'UpgradeItems')}
groups['DecorationItems'] = [item for group in shop['DecorationItems'] for item in group['Items']]
items = {x['Id']: x for group in groups.values() for x in group}
flowers = workshop['FlowerInfo']
papers = workshop['WrappingPaperInfo']
ribbons = workshop['RibbonInfo']
units = {x['Id']: x for x in flowers + papers + ribbons}
flower_map = {(int(x['FlowerType']), int(x['Color'])): x for x in flowers}
paper_map = {int(x['WrappingPaperType']): x for x in papers}
ribbon_map = {int(x['RibbonType']): x for x in ribbons}
customer_map = {x['Name']: x for x in customers['Customers']}
defaults = customers['DefaultOrderHappiness']
tip_low, tip_high = float(defaults['TipPercentage']['x']), float(defaults['TipPercentage']['y'])
changes = {int(x['Key']): int(x['Value']) for x in defaults['HappinessChange']['_serializedList']}
score = min(100, max(0, int(defaults['InitialSatisfaction']) + changes[32]))
threshold = int(defaults['HappinessTipLimit'])
tip_fraction = (tip_low + (tip_high - tip_low) * (score - threshold) / (100 - threshold)) / 100 if score >= threshold else 0

assert len(items) == 62
assert len(flower_map) == len(flowers) == len(groups['FlowerItems']) == 23
assert {x['Id'] for x in flowers} == {x['Id'] for x in groups['FlowerItems']}
assert {x['Id'] for x in papers} == {x['Id'] for x in groups['WrapperItems']}
assert {x['Id'] for x in ribbons} == {x['Id'] for x in groups['RibbonItems']}
assert all(int(x['Price']) > int(x['Cost']) >= 0 for x in units.values())
available = [x for x in items.values() if x.get('PurchaseDisabled', '0') != '1']
daily = {}
for item in available:
    if int(item['DefaultItemState']) < 2:
        daily.setdefault(int(item['UnlockDay']) + 1, []).append(item)
assert sorted(daily) == list(range(2, 53)), 'Daily release gaps or unintended releases'
assert all(len(x) == 1 for x in daily.values()), 'More than one new release per day'
assert items['u_1']['PurchaseDisabled'] == '1'
assert all(x['Prefab']['fileID'] != '0' for x in flowers)

def generated(owned, rng):
    pool = [x for x in flowers if x['Id'] in owned]
    main = rng.choice(pool)
    if rng.random() < float(economy['LatestFlowerOrderChance']):
        main = max(pool, key=lambda x: int(items[x['Id']]['UnlockDay']))
    other = [x for x in pool if x['FlowerType'] != main['FlowerType']]
    filler = rng.choice(other) if other else main
    paper = rng.choice([x for x in papers if x['Id'] in owned])
    ribbon = rng.choice([x for x in ribbons if x['Id'] in owned])
    return [[(main, int(economy['RandomOrderFlowerCount']) - 1), (filler, 1), (paper, 1), (ribbon, 1)]]

def authored(customer, owned):
    result = []
    for order in customer['Orders']:
        if order['BouquetType'] != '1' or not order['CustomFlowers']:
            return None
        bouquet = []
        for f in order['CustomFlowers']:
            item = flower_map.get((int(f['FlowerType']), int(f['FlowerColor'])))
            if not item or item['Id'] not in owned: return None
            bouquet.append((item, int(f['Count'])))
        for item in (paper_map.get(int(order['WrappingPaperType'])), ribbon_map.get(int(order['RibbonType']))):
            if not item or item['Id'] not in owned: return None
            bouquet.append((item, 1))
        result.append(bouquet)
    return result or None

def run(seed, served_fraction=1, tips=True):
    rng = random.Random(seed)
    owned = {x['Id'] for x in items.values() if int(x['DefaultItemState']) >= 2}
    cash = float(economy['StartingMoney'])
    history = []
    misses = 0
    for day in range(1, 53):
        spend = 0
        for item in available:
            if item['Id'] not in owned and int(item['UnlockDay']) < day and cash >= int(item['Price']):
                cash -= int(item['Price']); spend += int(item['Price']); owned.add(item['Id'])
        misses += sum(x['Id'] not in owned for x in daily.get(day, []))
        events = level['Days'][day - 1]['Events'] if day <= len(level['Days']) else level['RandomDayInfo']['Events']
        events = [x for x in events if x['IsEvent'] == '0']
        served = max(1, math.ceil(len(events) * served_fraction))
        revenue = costs = tip = 0
        for event in events[:served]:
            c = customer_map.get(event['CustomerName']) if event['CustomerType'] == '3' else None
            orders = authored(c, owned) if c else None
            if orders is None: orders = generated(owned, rng)
            order_price = sum(int(item['Price']) * n for bouquet in orders for item, n in bouquet)
            revenue += order_price
            costs += sum(int(item['Cost']) * n for bouquet in orders for item, n in bouquet)
            tip += round(order_price * tip_fraction) if tips else 0
        profit = revenue - costs + tip - int(economy['DailyRent'])
        cash += profit
        history.append(dict(day=day, served=served, revenue=revenue, costs=costs, tip=tip, profit=profit, spend=spend, cash=cash))
    return history, misses

normal = [run(seed) for seed in range(1000)]
gentle = [run(seed, .6, False) for seed in range(1000)]
assert sum(misses for _, misses in normal) == 0, 'Baseline cannot buy all daily releases'
assert sum(misses for _, misses in gentle) == 0, 'Low-throughput scenario has a daily purchase gate'
assert min(row['cash'] for history, _ in gentle for row in history) >= 0

lines = ['# Ekonomi simülasyonu', '',
    'Gerçek config assetlerinden üretilmiştir. 1.000 sabit rastgele tohum; 52 oyun günü. Oynanış testi değildir.', '',
    f'Normal senaryo: tüm planlı müşteriler, doğru sipariş/para üstü, {score}/100 memnuniyet, %{tip_fraction * 100:g} bahşiş. Merge ve reklam geliri **0**.',
    'Düşük tempo: günlük müşterilerin %60’ı (yukarı yuvarlanır), bahşiş **0**, Merge ve reklam geliri **0**. Servis edilmeyen müşteri için malzeme hazırlanmadığı varsayılır.',
    'Gün sayacı takvim günü değil, tamamlanan oyun günüdür. Her açılan ürün alınır; kira configteki değerdir.', '',
    '| Gün | Müşteri | Ciro ort. | Maliyet ort. | Bahşiş ort. | Net kazanç ort. | Yeni ürün bedeli | Gün sonu bakiye ort. | Düşük tempo net ort. |',
    '|---:|---:|---:|---:|---:|---:|---:|---:|---:|']
for day in list(range(1, 9)) + [14, 21, 30, 40, 41, 51, 52]:
    rows = [h[day-1] for h, _ in normal]
    avg = lambda field: statistics.mean(x[field] for x in rows)
    low = statistics.mean(h[day-1]['profit'] for h, _ in gentle)
    lines.append(f"| {day} | {avg('served'):g} | {avg('revenue'):.1f} | {avg('costs'):.1f} | {avg('tip'):.1f} | {avg('profit'):.1f} | {avg('spend'):g} | {avg('cash'):.1f} | {low:.1f} |")
lines += ['', f"Her iki senaryoda da toplam geciken günlük satın alma: **{sum(m for _, m in normal) + sum(m for _, m in gentle)}**.",
          f"52. gün ortalama bakiye: normal **{statistics.mean(h[-1]['cash'] for h, _ in normal):.0f}**, düşük tempo **{statistics.mean(h[-1]['cash'] for h, _ in gentle):.0f}**.",
          'Bu rahat denge, zorunlu para satın alımı veya zorunlu Merge ihtiyacı oluşturmaz. Hatalı hazırlanıp atılan buketler, oyuncunun kasada fazla para üstü vermesi ve gün bitiminde yarım kalan siparişler modele dahil değildir.', '',
          '## Ürün fiyatları ve açılış takvimi', '',
          'Shop bedeli bir kere ödenir. Birim maliyet/satış fiyatı her buketteki kullanım başınadır. İlk gün seçili dekorlar ve başlangıç malzemeleri ücretsizdir.', '',
          '| Oyuncu günü | ID | Ürün | Shop bedeli | Birim maliyet | Birim satış |',
          '|---:|---|---|---:|---:|---:|']
for item in sorted(items.values(), key=lambda x: (int(x['UnlockDay']), x['Id'])):
    id = item['Id']; unit = units.get(id)
    day = 'Yakında' if item.get('PurchaseDisabled') == '1' else str(int(item['UnlockDay']) + 1)
    lines.append(f"| {day} | {id} | {item['Name']} | {item['Price']} | {unit['Cost'] if unit else '—'} | {unit['Price'] if unit else '—'} |")
args.report.parent.mkdir(parents=True, exist_ok=True)
args.report.write_text('\n'.join(lines) + '\n', encoding='utf-8')
print(f'PASS: 62 items, 23 flower mappings, daily releases 2–52, positive unit margins.')
print(f'PASS: 2,000 x 52-day runs, no delayed daily purchases in either modeled scenario.')
print(f'Report: {args.report}')
