# 📂 Assets Folder Scanner

**A Unity Editor tool to scan your `Assets/` folder and display asset usage per folder with type-based color coding and interactive selection.**

## ✨ Features

- 🧠 Scans all files under `Assets/` and groups by folder
- 🎨 Color-coded folders based on dominant asset type
- 📊 Sort folders by natural order, asset type, or asset count
- 🎯 Filter folders with less than X assets
- 🖱️ Clickable entries to ping folders in the Project window
- 🧼 Clean, readable interface
- ✅ Compatible with Unity 2021.3+

## 🚀 Installation

### Using Unity Package Manager (Git URL)

1. Open `Packages/manifest.json`
2. Add the following line to your dependencies:

```json
"com.juliennoe.assets-scanner": "https://github.com/juliennoe/assets-folder-scanner.git"
```

Or download and place the folder in `Packages/AssetsFolderScanner`.

## 📁 Folder Colors by Type

| Type               | Color     |
|--------------------|-----------|
| `.png`             | Green     |
| `.jpg` / `.jpeg`   | Red       |
| Scripts (`.cs`)    | Yellow    |
| Audio (`.mp3`, `.wav`) | Blue  |
| Shaders (`.shader`) | Magenta  |
| Prefabs            | Cyan      |
| Materials          | White     |
| ScriptableObjects  | Light green |
| Other              | Gray      |

## 🧑‍💻 Author

Julien Noé  
[GitHub](https://github.com/juliennoe)

## 📄 License

MIT License – see [LICENSE](LICENSE) for full details.
