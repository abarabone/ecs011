# ecs_motion
- キャラクターアニメーション - ver.001
  - animation clip を motion clip アセットにコンバート
  - ボーントランスフォーム２形式
    - ルートから全ボーンをループで積算　<- こちらのほうがパフォーマンスがよさそう
    - ボーンを階層レベルごとにシステム化して積算
  - ブレンド未実装
- オーサリング
  - GameObjectConversionSystem 等も検討したが、LocalToWorld や余計なコンポーネントデータが付加するのでやめた
- physics
  - 地面、障害物との衝突を実装予定
