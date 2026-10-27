# Olympus 505 — `Modifiers/firearmMags.json` 에서 없는 총기 12개 제거

2026-10-27. 8-C 설치 중 서버 테스트에서 아래 오류가 났다.

```
[Olympus][AddMagazinesToFirearms] Error: The given key '<id>' was not present in the dictionary.
```

Olympus 가 자기 데이터에 적힌 총기에 탄창 호환을 추가하는데, 그 총기가 설치본에 없으면
예외가 나고 **거기서 반복문이 멈춘다.** 원래 222개 중 207개까지만 처리되고 나머지는 적용되지
않았다.

## 조치

설치본에 존재하지 않는 항목 **12개**를 `E:\Spt\SPT_Runtime\user\mods\MountOlympus\Modifiers\firearmMags.json`
에서 뺐다. 222 → 209 항목, 서버 테스트에서 **209개 전부 처리, 오류 0**.

제거한 id:

```
6932abeb5403890d0c09c926   (처음 걸린 것)
67c6de3ce39861860909e8e5
6981d72ed009ad83920da43a
69f9ebbcaae020b0db02f65d
6a15ae2ae5267ba21c07f98f
6a3bffbebc377285900b85cf
6871284e9a353bb50606f3ed
68c2940aecc41cc5490bd40e
68a639748e1fe612970728e9
68c16b9ab6b75a8a480520a6
68a6399922b1e0bd360afe56
689166b6c2d6fa42e7044756
68aee763130c00663d08aea8
```

(위 13줄 중 `6932abeb…` 를 먼저 빼고, 바닐라 DB·다른 모드 데이터 어디에도 없는 5개를 추려
뺐으며, 나머지는 서버 테스트를 돌려가며 걸리는 것을 하나씩 뺐다. 합계 12개가 실제로 제거됐다.)

## 되돌리기

원본은 [505-firearmMags-original.json](505-firearmMags-original.json) 에 있다. 그대로
덮어쓰면 설치 직후 상태로 돌아간다.

## 주의

**Olympus 를 다시 설치하거나 갱신하면 이 수정이 사라진다.** 그때 같은 오류가 나면 이 문서의
절차를 반복한다. 제거한 id 는 이 설치본에 없는 총기이므로, 해당 총기를 추가하는 모드를
나중에 넣으면 그 항목을 되살리는 편이 낫다.

이건 모드 제작자에게 보고할 거리이기도 하다 — 없는 총기 하나 때문에 반복문 전체가 멈추는
것은 Olympus 쪽에서 `continue` 로 넘기면 끝날 일이다.
