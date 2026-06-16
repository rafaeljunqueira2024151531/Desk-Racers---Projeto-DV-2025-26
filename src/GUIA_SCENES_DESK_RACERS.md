# Desk Racers - guia de scenes e ligacoes

## Quantas scenes criar

Cria 4 scenes:

1. `MainMenu`
   - Menu principal, opcoes, creditos e escolha de pista.
2. `Track_SetupGamer`
   - Pista da secretaria gamer.
3. `Track_KitchenBanquet`
   - Pista da cozinha.
4. `Loading`
   - Opcional, mas util para textos de lore. Se o tempo apertar, podes nao usar.

No `File > Build Profiles > Scene List`, mete pelo menos:

1. `MainMenu`
2. `Track_SetupGamer`
3. `Track_KitchenBanquet`

## Antes de comecar: limpar a SampleScene antiga

A `SampleScene` actual ainda pode ter objectos antigos criados pelo prototipo anterior.
Se vires erros de `Missing Script`, `DeskRacersCarController`, `RaceGameManager`, `TrackTrigger` ou `DeskRacersPickup`, ignora essa scene e faz isto:

1. `File > New Scene`.
2. Escolhe uma scene vazia.
3. `File > Save As...`
4. Guarda como `MainMenu`.
5. Repete para `Track_SetupGamer` e `Track_KitchenBanquet`.
6. Nao uses a `SampleScene` na entrega.

Se aparecer o erro:

`You are trying to read Input using the UnityEngine.Input class...`

entao o problema e o `EventSystem` antigo. Em cada scene com UI:

1. Selecciona `EventSystem`.
2. Remove o componente `Standalone Input Module`.
3. Adiciona o componente `Input System UI Input Module`.

Isto e necessario porque o projecto esta configurado para o novo Input System.

## Scene MainMenu

### Objectos principais

- `Canvas`
  - Titulo: Desk Racers
  - Botao `Jogar Setup Gamer`
  - Botao `Jogar Banquete da Cozinha`
  - Botao `Opcoes`
  - Botao `Creditos`
  - Botao `Sair`
  - Painel `OptionsPanel`
    - Slider volume
    - Slider sensibilidade
  - Painel `CreditsPanel`
    - nomes da equipa e docentes
- `EventSystem`
- `MainMenuManager`
  - Script: `MainMenuController`

### Ligacoes no Inspector

No `MainMenuManager`:

- `Options Panel` -> arrasta `OptionsPanel`
- `Credits Panel` -> arrasta `CreditsPanel`
- `Volume Slider` -> arrasta o slider de volume
- `Sensitivity Slider` -> arrasta o slider de sensibilidade

Nos botoes:

- `Jogar Setup Gamer`
  - `OnClick > MainMenuManager > MainMenuController.PlayTrack`
  - argumento: `Track_SetupGamer`
- `Jogar Banquete da Cozinha`
  - argumento: `Track_KitchenBanquet`
- `Opcoes`
  - `MainMenuController.ToggleOptions`
- `Creditos`
  - `MainMenuController.ToggleCredits`
- `Sair`
  - `MainMenuController.QuitGame`
- Slider volume
  - `On Value Changed > MainMenuController.SetVolume`
- Slider sensibilidade
  - `On Value Changed > MainMenuController.SetSensitivity`

## Estrutura comum das scenes de pista

Em cada pista cria esta hierarquia:

- `RaceManager`
  - Script: `RaceManager`
- `PlayerCar`
  - Modelo do carro
  - `Rigidbody`
  - `BoxCollider` ou colliders do modelo
  - Script: `ArcadeCarController`
  - Script: `ApplySavedSensitivity`
- `Main Camera`
  - Script: `FollowCamera`
- `HUD Canvas`
  - Text velocidade
  - Text volta
  - Text posicao
  - Text power-up
  - Text mensagens
  - `PausePanel`
    - Botao Resume
    - Botao Save
    - Botao Load
    - Botao Restart
    - Botao Menu
- `EventSystem`
- `Checkpoints`
  - `FinishLine`
  - `Checkpoint_0`
  - `Checkpoint_1`
  - `Checkpoint_2`
  - `Checkpoint_3`
- `Pickups`
- `Hazards`
- `TrackGeometry`

### Ligacoes no RaceManager

No `RaceManager`:

- `Player` -> arrasta `PlayerCar`
- `Track Name` -> nome da pista
- `Total Laps` -> 3
- `Checkpoint Count` -> numero de checkpoints normais, por exemplo 4
- `Speed Text` -> texto da velocidade
- `Lap Text` -> texto das voltas
- `Position Text` -> texto da posicao
- `Power Up Text` -> texto do power-up
- `Message Text` -> texto de mensagens
- `Pause Panel` -> painel de pausa

Nos botoes do `PausePanel`:

- Resume -> `RaceManager.TogglePause`
- Save -> `RaceManager.SaveGame`
- Load -> `RaceManager.LoadGame`
- Restart -> `RaceManager.RestartRace`
- Menu -> `RaceManager.BackToMenu`

### Ligacoes no PlayerCar

No `PlayerCar`:

- `ArcadeCarController`
  - ajusta `Acceleration`, `Max Speed`, `Turn Speed`, `Normal Grip`
- `ApplySavedSensitivity`
  - campo `Car` -> arrasta o proprio `PlayerCar`

No `Main Camera`:

- `FollowCamera`
  - campo `Target` -> arrasta `PlayerCar`

## Checkpoints e voltas

Cada checkpoint deve ser um cubo invisivel:

- adiciona `BoxCollider`
- marca `Is Trigger`
- desliga o `MeshRenderer` se quiseres invisivel
- adiciona `RaceCheckpoint`

Na meta:

- `Is Finish Line` = true
- `Race Manager` -> arrasta `RaceManager`

Nos checkpoints normais:

- `Is Finish Line` = false
- `Checkpoint Index`:
  - `Checkpoint_0` -> 0
  - `Checkpoint_1` -> 1
  - `Checkpoint_2` -> 2
  - `Checkpoint_3` -> 3
- `Race Manager` -> arrasta `RaceManager`

## Pista 1 - Track_SetupGamer

### Ideia visual

Uma secretaria enorme. O carro e pequeno, por isso os objectos devem parecer gigantes.

### Objectos que deves meter

- Base grande de madeira: secretaria.
- Tapete de rato preto: zona principal de drift.
- Teclado: zona de lombas.
- Monitor/PC: decoracao de escala.
- Ventoinha do PC: zona de vento.
- Cabos USB: obstaculos curvos.
- Caneca, livros, rato, auscultadores: barreiras/props.

### Mecânicas a ligar

Tapete de rato:

- cria um cubo fino por cima do tapete
- `BoxCollider > Is Trigger`
- script `SurfaceZone`
- `Grip Multiplier` entre `0.55` e `0.75`

Ventoinha:

- cria um cubo invisivel na frente da ventoinha
- `BoxCollider > Is Trigger`
- script `FanZone`
- `World Force`, por exemplo `(-12, 0, 3)`

Teclado:

- usa teclas individuais como cubos com collider
- faz pequenas alturas diferentes para parecer lombas
- deixa uma linha de passagem, senao o carro fica preso

Pickups:

- cria esferas/capsulas pequenas
- `SphereCollider > Is Trigger`
- script `PickupItem`
- para turbo: `Power Up Type = Turbo`, `Coin Amount = 0`
- para mola: `Power Up Type = Spring`, `Coin Amount = 0`
- para moeda: `Coin Amount = 1`

## Pista 2 - Track_KitchenBanquet

### Ideia visual

Uma bancada ou mesa de cozinha gigante, com pratos e talheres a formar a pista.

### Objectos que deves meter

- Mesa/bancada grande.
- Pratos como rotundas ou curvas.
- Talheres como barreiras compridas.
- Guardanapos como rampas suaves.
- Migalhas como pequenos ressaltos.
- Poca de agua/sumo para aquaplanagem.
- Torradeira como obstaculo dinamico.

### Mecânicas a ligar

Agua derramada:

- cubo fino invisivel em cima da zona molhada
- `BoxCollider > Is Trigger`
- script `SurfaceZone`
- `Grip Multiplier` entre `0.2` e `0.4`

Torradeira:

- GameObject `Toaster`
- filho `ToastSpawnPoint` virado para a pista
- script `ToasterLauncher` no `Toaster`
- `Toast Prefab` -> prefab da torrada
- `Spawn Point` -> `ToastSpawnPoint`
- `Launch Interval` -> 3

Prefab da torrada:

- modelo/cubo achatado
- `Rigidbody`
- `BoxCollider`
- script `DestroyAfterSeconds`

## Controlos

Teclado:

- `W/S` ou setas: acelerar/travar
- `A/D` ou setas: virar
- `Espaco`: drift
- `Shift Esquerdo`: power-up
- `Esc`: pausa
- `F1`: velocidade maxima
- `F2`: desbloquear pistas
- `F3`: ghost mode
- `F4`: turbo ilimitado

Comando PlayStation / gamepad:

- `R2`: acelerar
- `L2`: travar/marcha atras
- analogico esquerdo: virar
- `Circulo`: drift
- `X`: power-up
- `Options`: pausa

## Assets recomendados

Procura na Asset Store por estes termos:

- `free low poly car`
- `arcade vehicle`
- `toy car`
- `kitchen props`
- `food props`
- `office props`
- `keyboard`
- `computer desk`
- `low poly household`
- `cartoon props`

Prioridade:

1. Um carro simples com rodas visiveis.
2. Props de cozinha.
3. Props de escritorio/gaming.
4. Sons: motor RC, colisao plastico, pickup, turbo.

Se um asset for pesado, usa so alguns prefabs e apaga o resto da scene demo.

## Dicas rapidas de personalizacao

- Escala: faz o carro pequeno e os objectos gigantes.
- Usa materiais simples: madeira, plastico, metal, borracha, agua.
- Mete setas/checkpoints visuais na pista para o jogador perceber o caminho.
- Faz as barreiras com objectos do tema, nao com paredes genericas.
- Mantem a pista larga; carros arcade precisam de espaco para drift.
- Em cada pista, garante uma volta completa testavel antes de decorar muito.
