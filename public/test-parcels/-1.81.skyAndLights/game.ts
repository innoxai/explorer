import {
  Vector3,
  Entity,
  Transform,
  engine,
  BoxShape,
  SphereShape,
  Quaternion,
  Material,
  Color3,
  CylinderShape,
  PlaneShape
} from 'decentraland-ecs/src'

function makeEntity(shape: any, position: Vector3, scale?: Vector3) {
  const ent = new Entity()
  const s = new shape()
  ent.set(s)
  ent.set(
    new Transform({
      position,
      scale
    })
  )
  engine.addEntity(ent)
  return ent
}

makeEntity(BoxShape, new Vector3(-2, 0, -2), new Vector3(1, 10, 1))

makeEntity(SphereShape, new Vector3(3, 3, 3), new Vector3(1, 1, 1))

const redBox = makeEntity(BoxShape, new Vector3(5, 0, 5), new Vector3(0.1, 10, 0.1))
const redMat = new Material()
redMat.albedoColor = Color3.Red()
redBox.set(redMat)

makeEntity(BoxShape, new Vector3(0, 5, 0), new Vector3(10, 1, 1))

const greenBox = makeEntity(BoxShape, new Vector3(0.5, 0.5, 0.5))
const greenMat = new Material()
greenMat.albedoColor = Color3.FromHexString('#00FF00')
greenBox.set(greenMat)

const pinkBox = makeEntity(BoxShape, new Vector3(9.5, 0.5, 9.5))
const pinkMat = new Material()
pinkMat.albedoColor = Color3.FromHexString('#EF2D5E')
pinkBox.set(pinkMat)

const yellowBox = makeEntity(BoxShape, new Vector3(9.5, 0.5, 0.5))
const yellowMat = new Material()
yellowMat.albedoColor = Color3.FromHexString('#FFC65D')
yellowBox.set(yellowMat)

const aquaBox = makeEntity(BoxShape, new Vector3(0.5, 0.5, 9.5))
const aquaMat = new Material()
aquaMat.albedoColor = Color3.FromHexString('#7BC8A4')
aquaBox.set(aquaMat)

const blueBox = makeEntity(BoxShape, new Vector3(4, 0.5, 3))
const blueMat = new Material()
blueMat.albedoColor = Color3.FromHexString('#4CC3D9')
blueBox.set(blueMat)
blueBox.get(Transform).rotation = Quaternion.Euler(0, 45, 0)

const pinkSphere = makeEntity(SphereShape, new Vector3(3, 1.25, 5), new Vector3(1.25, 1.25, 1.25))
pinkSphere.set(pinkMat)

const yellowCylinder = makeEntity(CylinderShape, new Vector3(1, 0.75, 3), new Vector3(1, 1.5, 1))
yellowCylinder.get(CylinderShape).radiusTop = 0.5
yellowCylinder.get(CylinderShape).radiusBottom = 0.5
yellowCylinder.set(yellowMat)

const aquaPlane = makeEntity(PlaneShape, new Vector3(0, 0, 4), new Vector3(4, 4, 4))
aquaPlane.get(Transform).rotation = Quaternion.Euler(-90, 0, 0)
aquaPlane.set(aquaMat)
