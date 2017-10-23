# Tweaker les couleurs

### Avant de commencer

Le projet utilise la version Unity 2017.2 Assure toi d'avoir au moins cette version 

### Recuperer le projet 

downloader le projet:
https://github.com/Redoxee/ArrowLink
![dlgif]

### Ouvrir le projet

La scene de jeu se trouve dans \ArrowLink-master\ArrowLink\Assets\Scenes
il faut ouvrir le fichier GameScene.unity avec Unity

![openproject]


### Modifier les couleurs

Toutes les couleurs sont regroupées dans une ColorCollection
les ColorCollection se trouvent dans 
\Assets\Data\ColorCollections

![colorColections]

### Changer les couleurs

Au lancement du jeu, j'applique la couleur qui se trouve dans l'objet Color Manager

![ColorManager]

Le plus simple pour essayer des couleurs sans casser l'existant c'est de copier coler une Collection existante, puis de l'assigner au colorManager

![NewColorCollection]

Tu peux ensuite changer les differentes couleurs du projet

![NewColors]

**__Atention ! par defaut, l'alpha est à 0 !__**

### Visualizer

Pour voir tes changement tu peux soit lancer le jeu, Ou tu peut utiliser le bouton Apply en bas de la color collection

![Tester]

### Note

Selon le moment ou tu recupere le projet, il se peut que je teste des trucs au niveau gameplay. 
Ne t'inquiette pas si le jeu te semble cassé au niveau des fonctionalitées (au pire, demande moi)

#### Have Fun =)

[dlgif]:DlProject.gif
[openproject]:OpenProject.gif
[colorColections]:ColorCollections.gif
[ColorManager]:ColorManager.gif
[NewColorCollection]:NewColorCollection.gif
[NewColors]:NewColors.gif
[Tester]:Tester.gif