"""Validate the serialized UI mistakes that are not caught by C# diagnostics."""
from pathlib import Path
import re
root=Path(__file__).resolve().parents[2]
def blocks(path):
 s=(root/path).read_text(encoding='utf-8-sig')
 return {int(re.search(r'&(-?\d+)',b)[1]):b for b in re.split(r'(?=--- !u!)',s)[1:]}
def children(b):return [int(x) for x in re.findall(r'fileID: (\d+)',re.search(r'm_Children:(.*?)m_Father:',b,re.S)[1])]
def guid(path):return re.search(r'^guid: (\w+)',(root/(path+'.meta')).read_text(),re.M)[1]
def component(bs,rt,key):
 go=re.search(r'm_GameObject: \{fileID: (\d+)',bs[rt])[1]
 return next(b for b in bs.values() if 'm_GameObject: {fileID: '+go+'}' in b and key in b)
s=blocks('Assets/_Florist/Scenes/MergeGame.unity')
order=children(s[1157244908])
assert order[0:2]==[751000000000000001,1244062737], 'Inventory dimmer/background must render before section grids'
assert order.index(750900000000000001)>order.index(1244062737)
assert 'm_ConstraintCount: 3' in s[797354907] and 'm_ConstraintCount: 3' in s[750900000000000002]
assert 'seedContainer: {fileID: 750900000000000001}' in s[1157244907]
def vector(b,key):return tuple(float(v) for v in re.search(key+r": \{x: ([^,]+), y: ([^}]+)",b).groups())
size=vector(s[750900000000000001],'m_SizeDelta');cell=vector(s[750900000000000002],'m_CellSize');spacing=vector(s[750900000000000002],'m_Spacing')
assert size[0]>=cell[0]*3+spacing[0]*2 and size[1]>=cell[1]*2+spacing[1], 'Seed grid must fit three columns and two rows' 
color=re.search(r'm_fontColor: \{r: ([^,]+), g: ([^,]+), b: ([^,]+), a: ([^}]+)',component(s,1469703638,'m_text:'))
assert all(float(x)<.6 for x in color.groups()[:3]), 'Energy text needs contrast against its pale bar' 
assert guid('Assets/_Merge/Sprites/buttons/close.png') in component(s,1052245392,'m_Sprite:')
r=blocks('Assets/_Merge/Prefabs/RecipeModal.prefab')
assert not any('RecipePage' in b or 'previousButton:' in b or 'nextButton:' in b for b in r.values())
assert 'm_Content: {fileID: 8992720596111974881}' in r[752100000000000011]
assert 'm_VerticalFit: 2' in r[752100000000000012]
assert guid('Assets/_Merge/Sprites/buttons/close.png') in component(r,2305181659180026966,'m_Sprite:')
c=blocks('Assets/_Merge/Prefabs/RecipeCard.prefab')
assert guid('Assets/_Merge/Sprites/recipe/recipe_white_bar.png') in component(c,9033145948923416535,'m_Sprite:')
assert guid('Assets/_Merge/Sprites/buttons/action.png') in component(c,4494805949392419447,'m_Sprite:')
c=blocks('Assets/_Merge/Prefabs/CustomerOrderCard.prefab')
order=children(c[528940619862337373])
assert order.index(730400000000000001)>order.index(6256340694354609320), 'Order text cannot render behind its bubble'
assert order.index(752200000000000001)<order.index(6491008159962760662)
for rt in [6256340694354609320,6966011595275346721,6491008159962760662]:assert 'm_PreserveAspect: 1' in component(c,rt,'m_Sprite:')
assert guid('Assets/_Florist/Fonts/para-ikon.asset') in component(c,4023967981270554055,'m_text:')
# Check local IDs and transform parenting for all edited scene/prefab files.
paths=['Assets/_Florist/Scenes/MergeGame.unity']+[str(p.relative_to(root)) for p in (root/'Assets/_Merge/Prefabs').glob('*.prefab')]
for path in paths:
 bs=blocks(path)
 for i,b in bs.items():
  for ref in re.findall(r'\{fileID: (-?\d+)\}',b):assert ref=='0' or int(ref) in bs,(path,i,ref)
  if b.startswith('--- !u!224') and 'm_Father:' in b:
   parent=int(re.search(r'm_Father: \{fileID: (\d+)',b)[1])
   if parent:assert i in children(bs[parent]),(path,i,parent)
print('UI checks passed: section visibility, 3+6 grid layout, close/action sprites, energy contrast, recipe scroll, customer layering/aspect, money sprite and local references.')
