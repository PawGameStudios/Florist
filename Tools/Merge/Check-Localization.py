"""Validate Merge localization keys, format arguments and explicitly wired labels."""
from pathlib import Path
import re,json
root=Path(__file__).resolve().parents[2]
langs={n:json.loads((root/f'Assets/Resources/Localization/{n}.json').read_text(encoding='utf-8-sig'))['items'] for n in ['tr-TR','en-US']}
keys={k for k in langs['tr-TR'] if k.startswith('merge_')}
assert keys=={k for k in langs['en-US'] if k.startswith('merge_')}
for k in keys:
 assert all(langs[n][k] for n in langs)
 assert re.findall(r'\{\d+(?::[^}]+)?\}',langs['tr-TR'][k])==re.findall(r'\{\d+(?::[^}]+)?\}',langs['en-US'][k]),k
sources=list((root/'Assets/_Merge/Scripts').glob('*.cs'))+list((root/'Assets/_Merge/ScriptableObjects').rglob('*.asset'))+[root/'Assets/_Florist/_Configs/MergeProgressionConfig.asset']
used=set()
for p in sources:used.update(re.findall(r'\bmerge_[a-z_0-9]+\b',p.read_text(encoding='utf-8-sig')))
labelguid=re.search(r'guid: (\w+)',(root/'Assets/_Merge/Scripts/MergeLocalizedLabel.cs.meta').read_text())[1]
count=0
for p in [root/'Assets/_Florist/Scenes/MergeGame.unity']+list((root/'Assets/_Merge/Prefabs').glob('*.prefab')):
 bs={int(re.search(r'&(-?\d+)',b)[1]):b for b in re.split(r'(?=--- !u!)',p.read_text(encoding='utf-8-sig'))[1:]}
 for b in bs.values():
  if labelguid not in b:continue
  key=re.search(r'localizationKey: (\w+)',b)[1];used.add(key)
  target=int(re.search(r'targetText: \{fileID: (\d+)',b)[1]);assert 'm_text:' in bs[target]
  assert re.search(r'm_GameObject: \{fileID: (\d+)',b)[1]==re.search(r'm_GameObject: \{fileID: (\d+)',bs[target])[1]
  count+=1
assert not used-keys,used-keys
scene=(root/'Assets/_Florist/Scenes/MergeGame.unity').read_text(encoding='utf-8-sig')
assert 'titleText:' not in scene
energyguid=re.search(r'guid: (\w+)',(root/'Assets/_Merge/Sprites/energy-icon.asset.meta').read_text())[1]
assert f'm_spriteAsset: {{fileID: 11400000, guid: {energyguid}' in scene
assert '{m_Energy} <sprite=0>' in (root/'Assets/_Merge/Scripts/GameManager.cs').read_text(encoding='utf-8-sig')
assert 'titleText' not in (root/'Assets/_Merge/Scripts/InventoryWindow.cs').read_text(encoding='utf-8-sig')
print(f'{len(keys)} matching TR/EN keys, format placeholders, {count} static label components, energy sprite and removed title references verified.')
