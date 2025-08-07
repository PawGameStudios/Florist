# Florist Game - Project Index

## Project Overview
This is a Unity-based mobile game called "Florist" where players run a flower shop, interact with customers, create bouquets in a workshop, and manage their business. The game features a Turkish/English localization system, Firebase integration, Google Mobile Ads, and in-app purchases.

## Project Structure

### Core Game Architecture

#### 1. **Main Systems** (`_Florist/Scripts/Main/`)
- **Bootstrapper.cs** - Game initialization entry point
- **MainPage.cs** - Main menu and navigation hub
- **EndDayPage.cs** - End-of-day summary and progression
- **TopCanvas.cs** - UI overlay for global elements
- **HamburgerPanel.cs** - Navigation menu

#### 2. **Shop Management** (`_Florist/Scripts/Dukkan/`)
- **DukkanPage.cs** - Main shop interface (558 lines)
- **Customer.cs** - Customer interaction system (725 lines)
- **Bouquet.cs** - Flower arrangement logic
- **DayTimeManager.cs** - Day progression and timing
- **HappinessMeter.cs** - Customer satisfaction tracking
- **DecorationManager.cs** - Shop customization
- **PosController.cs** - Point of sale system
- **MoneyObject.cs** - Currency handling

#### 3. **Workshop System** (`_Florist/Scripts/Workshop/`)
- **WorkshopPage.cs** - Main workshop interface (934 lines)
- **PaperArea.cs** - Wrapping paper management (422 lines)
- **WrappingMachine.cs** - Automated bouquet wrapping
- **RibbonTable.cs** - Ribbon selection and application
- **FlowerBox.cs** - Flower storage and selection
- **Flower.cs** - Individual flower properties
- **ConvoHistory.cs** - Conversation tracking

#### 4. **Configuration System** (`_Florist/Scripts/Configs/`)
- **Configs.cs** - Central configuration manager
- **CustomerConfig.cs** - Customer types and behaviors (152 lines)
- **WorkshopConfig.cs** - Workshop items and recipes (288 lines)
- **LevelConfig.cs** - Progression system
- **ProfileConfig.cs** - User profile settings

#### 5. **Data Management** (`_Florist/Scripts/Save/`)
- **SaveSystem.cs** - Save/load functionality
- **SaveData.cs** - Game state serialization (134 lines)
- **GeneralData.cs** - Global settings and preferences
- **ShopData.cs** - Shop-specific data (191 lines)
- **FileManager.cs** - File I/O operations
- **Resetter.cs** - Data reset functionality

#### 6. **Shop & Commerce** (`_Florist/Scripts/Shop/`)
- **ShopPage.cs** - Shop interface
- **ShopItem.cs** - Individual shop items (240 lines)
- **ShopButton.cs** - Shop UI interactions
- **ShopScroll.cs** - Shop navigation
- **ShopConfig.cs** - Shop configuration

#### 7. **Profile System** (`_Florist/Scripts/Profile/`)
- **Profile.cs** - User profile management
- **ProfileMenu.cs** - Profile UI

### Utility Systems

#### 8. **Core Utilities** (`_Florist/Scripts/Utils/`)
- **MonoSingleton.cs** - Singleton pattern implementation
- **References.cs** - Global component references
- **Page.cs** - Base page class for navigation
- **Extensions.cs** - Utility extension methods
- **Timer.cs** - Time management utilities
- **InputManager.cs** - Input handling
- **Popup.cs** - Popup dialog system

#### 9. **External Services** (`_Florist/Scripts/Utils/`)
- **FirebaseController.cs** - Firebase Analytics & Crashlytics (630 lines)
- **AdManager.cs** - Google Mobile Ads integration (517 lines)
- **SoundController.cs** - Audio management (219 lines)
- **HapticsController.cs** - Haptic feedback
- **PrivacyController.cs** - Privacy policy handling
- **RateUsController.cs** - App rating prompts

#### 10. **IAP System** (`_Florist/Scripts/Utils/IAP/`)
- **Purchaser.cs** - In-app purchase handling (269 lines)
- **IAPConfig.cs** - IAP configuration

#### 11. **Localization** (`_Florist/Scripts/Utils/_Localization/`)
- **LocalizationManager.cs** - Multi-language support (105 lines)
- **LocalizedTextMeshProUGUI.cs** - Localized UI text
- **LocalizedImage.cs** - Localized images
- **SerializationHandler.cs** - JSON serialization

#### 12. **Tutorial System** (`_Florist/Scripts/Tutorial/`)
- **Tutorial.cs** - Tutorial management (409 lines)
- **TutorialController.cs** - Tutorial coordination
- **HandAnimController.cs** - Tutorial animations

### Asset Organization

#### 13. **Configuration Assets** (`_Florist/_Configs/`)
- **CustomerConfig.asset** - Customer data (261 lines)
- **WorkshopConfig.asset** - Workshop items (448 lines)
- **ShopConfig.asset** - Shop items (331 lines)
- **LevelConfig.asset** - Level progression
- **ProfileConfig.asset** - Profile settings
- **AdsConfig.asset** - Ad configuration
- **_IAPConfig.asset** - IAP settings

#### 14. **Conversations** (`_Florist/Conversations/`)
- Customer dialogue files for different scenarios
- Initial conversations, goodbye conversations
- Happiness-based dialogue variations

#### 15. **Prefabs** (`_Florist/Prefabs/`)
- **Main/** - Main UI prefabs
- **Workshop/** - Workshop components (23 prefabs)
- **Shop/** - Shop UI elements
- **Profile/** - Profile interface

#### 16. **Sprites** (`_Florist/Sprites/`)
- **ana-ekran-ui/** - Main screen UI
- **atolye/** - Workshop graphics
- **cicekler/** - Flower sprites
- **dukkan_dis/** - Shop exterior
- **dukkan_ic/** - Shop interior
- **karakterler/** - Character sprites (48 files)
- **magaza/** - Shop graphics
- **tarif_kitabı/** - Recipe book
- **tutorial/** - Tutorial graphics
- **zaman-mutluluk-gostergeleri-yeni/** - Time/happiness indicators

### External Dependencies

#### 17. **Third-Party Packages**
- **Conversa** - Dialogue system
- **DOTween** - Animation system
- **Odin Inspector** - Enhanced editor
- **TextMesh Pro** - Text rendering
- **Firebase** - Analytics and crash reporting
- **Google Mobile Ads** - Advertisement system
- **JsonDotNet** - JSON serialization
- **SerializedCollections** - Serialized collections

#### 18. **Localization Files** (`Resources/Localization/`)
- **en-US.json** - English localization (96 entries)
- **tr-TR.json** - Turkish localization

### Key Game Mechanics

#### 19. **Core Gameplay Loop**
1. **Customer Interaction** - Customers enter shop with orders
2. **Order Processing** - Players create bouquets in workshop
3. **Quality Assessment** - Orders are evaluated for accuracy
4. **Payment & Tips** - Revenue based on customer satisfaction
5. **Shop Progression** - Unlock new items and upgrades

#### 20. **Workshop System**
- **Flower Selection** - Choose from various flower types and colors
- **Paper Wrapping** - Select wrapping paper designs
- **Ribbon Application** - Add decorative ribbons
- **Machine Processing** - Automated wrapping with upgrades
- **Quality Control** - Scissors for stem trimming

#### 21. **Customer System**
- **Customer Types** - Different customer personalities
- **Order Generation** - Random or specific bouquet requests
- **Happiness Tracking** - Customer satisfaction affects tips
- **Conversation System** - Dynamic dialogue based on outcomes

#### 22. **Economy System**
- **Revenue Tracking** - Sales, tips, refunds
- **Cost Management** - Flower costs, rent, upgrades
- **Profit Calculation** - Daily profit/loss tracking
- **Shop Upgrades** - Unlock new items and features

### Technical Architecture

#### 23. **Design Patterns**
- **Singleton Pattern** - Global managers (MonoSingleton)
- **Observer Pattern** - Event-driven communication
- **Factory Pattern** - Object creation (flowers, bouquets)
- **State Pattern** - Game state management

#### 24. **Data Flow**
- **Save System** - Persistent data management
- **Configuration** - ScriptableObject-based data
- **Localization** - JSON-based text management
- **Analytics** - Firebase event tracking

#### 25. **Performance Considerations**
- **Object Pooling** - Reusable game objects
- **Lazy Loading** - Load assets on demand
- **Memory Management** - Proper cleanup and disposal
- **UI Optimization** - Efficient UI updates

### Platform Integration

#### 26. **Mobile Features**
- **Touch Input** - Multi-touch support
- **Haptic Feedback** - Device vibration
- **Notifications** - Push notifications
- **Privacy Compliance** - GDPR/privacy policy

#### 27. **Monetization**
- **Ad Integration** - Banner, interstitial, rewarded ads
- **In-App Purchases** - Premium content and currency
- **Analytics** - User behavior tracking
- **Crash Reporting** - Error monitoring

### Development Tools

#### 28. **Editor Extensions**
- **Odin Inspector** - Enhanced property drawers
- **Custom Editors** - Specialized inspector views
- **Debug Tools** - Development utilities
- **Build Pipeline** - Automated build process

This index provides a comprehensive overview of the Florist game project, covering all major systems, components, and architectural decisions. The project demonstrates a well-structured Unity game with proper separation of concerns, modular design, and integration with modern mobile game services. 