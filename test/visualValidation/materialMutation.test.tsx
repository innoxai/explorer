// TODO: This needs to be reimplemented using ECS
// tslint:disable:no-commented-out-code

// import { testScene, saveScreenshot } from '../testHelpers'
// import { ScriptableScene, createElement } from 'decentraland-api/src'
// import { future } from 'fp-future'
// import { expect } from 'chai'

// const beauty =
//   'data:image/png;base64,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'

// describe('mutate material wrapper', () => {
//   const didSetMaterial = future()

//   class TestComponent extends ScriptableScene<{ color: string }> {
//     async render() {
//       if (this.props.color) {
//         didSetMaterial.resolve(this.props.color)
//       }
//       return (
//         <scene>
//           <material id="mat1" />
//           <material
//             id="mat2"
//             albedoColor={'#cccccc'}
//             metallic={0.5}
//             roughness={0.5}
//             emissiveColor={this.props.color || '#000000'}
//           />
//           <material
//             id="mat3"
//             albedoColor={'#00ff00'}
//             metallic={1}
//             roughness={0}
//             directIntensity={0.1}
//             reflectionColor="#ff00ff"
//             albedoTexture={beauty}
//             refractionTexture={beauty}
//           />
//           <cylinder position={{ x: 2, y: 1, z: 4 }} material="#nonExistent" color={this.props.color || '#0000ff'} />
//           <box position={{ x: 3, y: 1, z: 3 }} material="#mat2" color={this.props.color || '#000000'} />
//           <sphere position={{ x: 2, y: 1, z: 2 }} material="#mat1" color={this.props.color || '#000000'} />
//           <sphere position={{ x: 2, y: 0, z: 2 }} material="#mat3" />
//         </scene>
//       )
//     }
//   }

//   testScene('create and mutate material', TestComponent, (root, { parcelScenePromise, classPromise }) => {
//     saveScreenshot('materialMutation.0.png', { lookAt: [2, 1, 4], from: [0, 0, 0] })

//     it('changes the color of a material', async () => {
//       const newColor = '#ffeeaa'
//       const script = await parcelScenePromise
//       script.setAttributes({ color: newColor })
//       const settedColor = await didSetMaterial
//       expect(settedColor).to.eq(newColor)
//     })

//     saveScreenshot('materialMutation.1.png', { lookAt: [2, 1, 4], from: [0, 0, 0] })
//   })
// })
