using System;
using System.IO;
using DeskRacers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DeskRacersSceneBuilder
{
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string AutoBuildMarkerPath = "Assets/DeskRacersSceneBuilt.marker";

    [InitializeOnLoadMethod]
    static void AutoBuildOnce()
    {
        if (Application.isBatchMode || File.Exists(AutoBuildMarkerPath))
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (File.Exists(AutoBuildMarkerPath))
            {
                return;
            }

            BuildScene();
            File.WriteAllText(AutoBuildMarkerPath, $"Scene generated automatically at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AssetDatabase.ImportAsset(AutoBuildMarkerPath);
        };
    }

    [MenuItem("Desk Racers/Build Vertical Slice Scene")]
    public static void BuildScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(root);
        }

        Material desk = Material("Desk Warm Wood", new Color(0.55f, 0.34f, 0.18f));
        Material track = Material("Mousepad Track", new Color(0.07f, 0.08f, 0.09f));
        Material barrier = Material("Blue Plastic Barrier", new Color(0.08f, 0.32f, 0.9f));
        Material key = Material("Keyboard Keys", new Color(0.16f, 0.16f, 0.17f));
        Material red = Material("RC Red", new Color(0.95f, 0.08f, 0.05f));
        Material black = Material("Rubber Black", new Color(0.02f, 0.02f, 0.025f));
        Material yellow = Material("Pickup Yellow", new Color(1f, 0.82f, 0.08f));
        Material green = Material("Spring Green", new Color(0.2f, 0.9f, 0.25f));
        Material cyan = Material("Fan Cyan", new Color(0.15f, 0.8f, 1f));

        GameObject deskSurface = Cube("Giant Desk Surface", new Vector3(0f, -0.15f, 0f), new Vector3(42f, 0.3f, 30f), desk);
        deskSurface.tag = "Untagged";

        Cube("Mousepad Drift Zone", new Vector3(-6f, 0.03f, -3f), new Vector3(14f, 0.08f, 10f), track);
        BuildOvalBarriers(barrier);
        BuildKeyboard(key);
        BuildPropsAndHazards(cyan, green, yellow);

        GameObject car = BuildCar(red, black);
        GameObject camera = BuildCamera(car.transform);
        BuildLighting();
        BuildUi(car.GetComponent<DeskRacersCarController>());

        Selection.activeGameObject = car;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Desk Racers vertical slice scene built. Press Play and use W/S, A/D, Space, Left Shift, Esc, F1-F4.");
    }

    static void BuildOvalBarriers(Material barrier)
    {
        for (int i = 0; i < 18; i++)
        {
            float angle = i / 18f * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 14f, 0.35f, Mathf.Sin(angle) * 9f);
            GameObject block = Cube("Plastic Track Barrier", pos, new Vector3(2.6f, 0.7f, 0.55f), barrier);
            block.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
        }

        for (int i = 0; i < 14; i++)
        {
            float angle = i / 14f * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 7.5f, 0.35f, Mathf.Sin(angle) * 4.5f);
            GameObject block = Cube("Inner Plastic Barrier", pos, new Vector3(1.7f, 0.7f, 0.45f), barrier);
            block.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
        }

        GameObject lap = Cube("Start Finish Trigger", new Vector3(0f, 1.1f, -8.8f), new Vector3(5f, 2f, 0.35f), barrier);
        lap.GetComponent<Collider>().isTrigger = true;
        lap.AddComponent<TrackTrigger>().type = TrackTrigger.TriggerType.Lap;
    }

    static void BuildKeyboard(Material key)
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                float x = -12f + col * 1.15f + row * 0.18f;
                float z = 4.3f + row * 0.9f;
                Cube("Keyboard Bump Key", new Vector3(x, 0.18f, z), new Vector3(0.95f, 0.28f, 0.68f), key);
            }
        }
    }

    static void BuildPropsAndHazards(Material cyan, Material green, Material yellow)
    {
        GameObject fan = Cube("PC Fan Wind Zone", new Vector3(10f, 1.2f, 1.5f), new Vector3(5.5f, 2.4f, 4f), cyan);
        fan.GetComponent<Collider>().isTrigger = true;
        TrackTrigger fanTrigger = fan.AddComponent<TrackTrigger>();
        fanTrigger.type = TrackTrigger.TriggerType.Fan;
        fanTrigger.fanForce = new Vector3(-18f, 0f, 2f);

        GameObject water = Cube("Spilled Soda Slippery Zone", new Vector3(5f, 0.05f, -6.2f), new Vector3(5f, 0.08f, 2.8f), cyan);
        water.GetComponent<Collider>().isTrigger = true;
        water.AddComponent<TrackTrigger>().type = TrackTrigger.TriggerType.Slippery;

        GameObject toaster = Cube("Toaster Toast Obstacle", new Vector3(-9f, 0.7f, -6.5f), new Vector3(1.2f, 1.2f, 3.5f), yellow);
        toaster.AddComponent<OscillatingObstacle>().localMove = new Vector3(0f, 0f, 2.8f);

        CreatePickup("Spring Pickup", new Vector3(-4f, 0.8f, -8.2f), green, DeskRacersPickup.PickupType.Spring);
        CreatePickup("Turbo Pickup", new Vector3(8.5f, 0.8f, 5.7f), cyan, DeskRacersPickup.PickupType.Turbo);

        for (int i = 0; i < 10; i++)
        {
            float angle = i / 10f * Mathf.PI * 2f;
            CreatePickup("Mini Coin", new Vector3(Mathf.Cos(angle) * 10.5f, 0.75f, Mathf.Sin(angle) * 6.6f), yellow, DeskRacersPickup.PickupType.Coin);
        }
    }

    static void CreatePickup(string name, Vector3 position, Material material, DeskRacersPickup.PickupType type)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = name;
        pickup.transform.position = position;
        pickup.transform.localScale = Vector3.one * 0.55f;
        pickup.GetComponent<Renderer>().sharedMaterial = material;
        pickup.GetComponent<Collider>().isTrigger = true;
        pickup.AddComponent<DeskRacersPickup>().type = type;
    }

    static GameObject BuildCar(Material bodyMaterial, Material wheelMaterial)
    {
        GameObject car = new GameObject("RC Microcar Player");
        car.transform.position = new Vector3(0f, 0.6f, -7.5f);
        car.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Rigidbody rb = car.AddComponent<Rigidbody>();
        rb.mass = 0.55f;

        GameObject body = Cube("Body", Vector3.zero, new Vector3(1.35f, 0.35f, 2.05f), bodyMaterial);
        body.transform.SetParent(car.transform, false);
        body.transform.localPosition = new Vector3(0f, 0f, 0f);

        GameObject cabin = Cube("Cabin", Vector3.zero, new Vector3(0.85f, 0.35f, 0.8f), bodyMaterial);
        cabin.transform.SetParent(car.transform, false);
        cabin.transform.localPosition = new Vector3(0f, 0.34f, -0.2f);

        Vector3[] wheelPositions =
        {
            new Vector3(-0.78f, -0.18f, 0.65f),
            new Vector3(0.78f, -0.18f, 0.65f),
            new Vector3(-0.78f, -0.18f, -0.65f),
            new Vector3(0.78f, -0.18f, -0.65f)
        };

        foreach (Vector3 wheelPosition in wheelPositions)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Rubber Wheel";
            wheel.transform.SetParent(car.transform, false);
            wheel.transform.localPosition = wheelPosition;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.28f, 0.18f, 0.28f);
            wheel.GetComponent<Renderer>().sharedMaterial = wheelMaterial;
            Object.DestroyImmediate(wheel.GetComponent<Collider>());
        }

        BoxCollider collider = car.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.35f, 0.55f, 2.05f);
        collider.center = new Vector3(0f, 0.05f, 0f);
        car.AddComponent<DeskRacersCarController>();
        return car;
    }

    static GameObject BuildCamera(Transform target)
    {
        GameObject camera = new GameObject("Main Camera");
        Camera cam = camera.AddComponent<Camera>();
        cam.fieldOfView = 62f;
        camera.tag = "MainCamera";
        FollowCamera follow = camera.AddComponent<FollowCamera>();
        follow.target = target;
        camera.transform.position = target.position + new Vector3(0f, 5f, -7f);
        return camera;
    }

    static void BuildLighting()
    {
        GameObject light = new GameObject("Desk Lamp Key Light");
        Light lamp = light.AddComponent<Light>();
        lamp.type = LightType.Directional;
        lamp.intensity = 1.8f;
        light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.42f);
    }

    static void BuildUi(DeskRacersCarController player)
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        GameObject canvasObject = new GameObject("HUD Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Text speed = HudText(canvasObject.transform, "Speed", "000 cm/s", new Vector2(22f, -22f), TextAnchor.UpperLeft, font);
        Text position = HudText(canvasObject.transform, "Position", "1/4", new Vector2(-22f, -22f), TextAnchor.UpperRight, font);
        Text lap = HudText(canvasObject.transform, "Lap", "1/3", new Vector2(22f, -62f), TextAnchor.UpperLeft, font);
        Text power = HudText(canvasObject.transform, "Powerup", "Mola", new Vector2(-22f, -62f), TextAnchor.UpperRight, font);
        Text message = HudText(canvasObject.transform, "Message", string.Empty, new Vector2(0f, -110f), TextAnchor.UpperCenter, font);
        message.rectTransform.anchorMin = new Vector2(0.2f, 1f);
        message.rectTransform.anchorMax = new Vector2(0.8f, 1f);
        message.rectTransform.sizeDelta = new Vector2(0f, 42f);

        GameObject mainMenuPanel = Panel(canvasObject.transform, "Main Menu Panel", new Color(0f, 0f, 0f, 0.78f));
        GameObject pausePanel = Panel(canvasObject.transform, "Pause Panel", new Color(0f, 0f, 0f, 0.72f));
        GameObject creditsPanel = Panel(canvasObject.transform, "Credits Panel", new Color(0f, 0f, 0f, 0.82f));
        pausePanel.SetActive(false);
        creditsPanel.SetActive(false);

        Text mainTitle = ChildText(mainMenuPanel.transform, "Title", "DESK RACERS", new Vector2(0f, 140f), new Vector2(360f, 48f), TextAnchor.MiddleCenter, font, 34);
        mainTitle.color = new Color(1f, 0.82f, 0.25f);
        ChildText(mainMenuPanel.transform, "Subtitle", "Corridas secretas de microcarros pela casa", new Vector2(0f, 100f), new Vector2(380f, 32f), TextAnchor.MiddleCenter, font, 16);

        Text title = ChildText(pausePanel.transform, "Title", "DESK RACERS", new Vector2(0f, 145f), new Vector2(360f, 42f), TextAnchor.MiddleCenter, font, 30);
        title.color = new Color(1f, 0.82f, 0.25f);

        RaceGameManager manager = new GameObject("Race Game Manager").AddComponent<RaceGameManager>();
        manager.player = player;
        manager.speedText = speed;
        manager.positionText = position;
        manager.lapText = lap;
        manager.powerUpText = power;
        manager.messageText = message;
        manager.mainMenuPanel = mainMenuPanel;
        manager.pausePanel = pausePanel;
        manager.creditsPanel = creditsPanel;

        Button play = Button(mainMenuPanel.transform, "Play Button", "Jogar", new Vector2(0f, 45f), font);
        play.onClick.AddListener(manager.StartRace);
        Button options = Button(mainMenuPanel.transform, "Options Button", "Opcoes", new Vector2(0f, -5f), font);
        options.onClick.AddListener(delegate { pausePanel.SetActive(true); });
        Button credits = Button(mainMenuPanel.transform, "Credits Button", "Creditos", new Vector2(0f, -55f), font);
        credits.onClick.AddListener(manager.ShowCredits);
        Button quitMain = Button(mainMenuPanel.transform, "Quit Main Button", "Sair", new Vector2(0f, -105f), font);
        quitMain.onClick.AddListener(manager.QuitGame);

        ChildText(creditsPanel.transform, "Credits Title", "FICHA TECNICA", new Vector2(0f, 130f), new Vector2(360f, 40f), TextAnchor.MiddleCenter, font, 28);
        ChildText(creditsPanel.transform, "Credits Text", "LEI - Desenvolvimento de Videojogos\nPL4\nDiogo Brito, Diogo Gomes, Rafael Junqueira\nDocentes: Prof. Joao Morais, Prof. Joao Silva", new Vector2(0f, 30f), new Vector2(360f, 130f), TextAnchor.MiddleCenter, font, 18);
        Button closeCredits = Button(creditsPanel.transform, "Close Credits Button", "Voltar", new Vector2(0f, -125f), font);
        closeCredits.onClick.AddListener(manager.HideCredits);

        Button save = Button(pausePanel.transform, "Save Button", "Save", new Vector2(0f, 75f), font);
        save.onClick.AddListener(manager.SaveGame);
        Button load = Button(pausePanel.transform, "Load Button", "Load", new Vector2(0f, 25f), font);
        load.onClick.AddListener(manager.LoadGame);
        Button resume = Button(pausePanel.transform, "Resume Button", "Resume", new Vector2(0f, -25f), font);
        resume.onClick.AddListener(manager.TogglePause);
        Button quit = Button(pausePanel.transform, "Quit Button", "Sair", new Vector2(0f, -75f), font);
        quit.onClick.AddListener(manager.QuitGame);

        manager.volumeSlider = Slider(pausePanel.transform, "Volume Slider", "Volume", new Vector2(0f, -135f), 0f, 1f, 1f, font);
        manager.sensitivitySlider = Slider(pausePanel.transform, "Sensitivity Slider", "Sensibilidade", new Vector2(0f, -185f), 60f, 180f, player.turnStrength, font);
    }

    static Text HudText(Transform parent, string name, string text, Vector2 offset, TextAnchor anchor, Font font)
    {
        Text label = ChildText(parent, name, text, offset, new Vector2(220f, 36f), anchor, font, 24);
        if (anchor == TextAnchor.UpperRight)
        {
            label.rectTransform.anchorMin = new Vector2(1f, 1f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.pivot = new Vector2(1f, 1f);
        }
        else if (anchor == TextAnchor.UpperCenter)
        {
            label.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            label.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            label.rectTransform.pivot = new Vector2(0.5f, 1f);
        }
        else
        {
            label.rectTransform.anchorMin = new Vector2(0f, 1f);
            label.rectTransform.anchorMax = new Vector2(0f, 1f);
            label.rectTransform.pivot = new Vector2(0f, 1f);
        }

        return label;
    }

    static GameObject Panel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(430f, 470f);
        return panel;
    }

    static Button Button(Transform parent, string name, string text, Vector2 position, Font font)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.1f, 0.18f, 0.28f);
        Button button = buttonObject.AddComponent<Button>();
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(220f, 38f);
        ChildText(buttonObject.transform, "Text", text, Vector2.zero, rect.sizeDelta, TextAnchor.MiddleCenter, font, 20);
        return button;
    }

    static Slider Slider(Transform parent, string name, string label, Vector2 position, float min, float max, float value, Font font)
    {
        ChildText(parent, label + " Label", label, position + new Vector2(-95f, 20f), new Vector2(160f, 24f), TextAnchor.MiddleLeft, font, 16);
        GameObject sliderObject = new GameObject(name);
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(220f, 18f);

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObject.transform, false);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.15f, 0.15f, 0.15f);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(sliderObject.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(1f, 0.72f, 0.18f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.sizeDelta = Vector2.zero;
        slider.fillRect = fillRect;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObject.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(18f, 26f);
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        return slider;
    }

    static Text ChildText(Transform parent, string name, string text, Vector2 position, Vector2 size, TextAnchor anchor, Font font, int fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.alignment = anchor;
        label.color = Color.white;
        RectTransform rect = label.rectTransform;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return label;
    }

    static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    static Material Material(string name, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = name;
        material.color = color;
        return material;
    }
}
