export const typeDefs = `#graphql
  scalar DateTime

  type SensorReading {
    id: ID!
    timestamp: DateTime!
    deviceId: String!
    location: String
    cropType: String
    season: String
    temperature: Float
    humidity: Float
    rainfall: Float
    soilMoisture: Float
    soilPh: Float
    lightIntensity: Float
    fertilizerUsed: Float
    irrigationNeeded: Boolean
    cropHealth: String
    yieldEstimate: Float
    pestRisk: String
    anomalyFlag: Boolean
  }

  input SensorReadingInput {
    timestamp: DateTime!
    deviceId: String!
    location: String
    cropType: String
    season: String
    temperature: Float
    humidity: Float
    rainfall: Float
    soilMoisture: Float
    soilPh: Float
    lightIntensity: Float
    fertilizerUsed: Float
    irrigationNeeded: Boolean
    cropHealth: String
    yieldEstimate: Float
    pestRisk: String
    anomalyFlag: Boolean
  }

  type Query {
    sensorReadings(limit: Int = 100, offset: Int = 0): [SensorReading!]!
    sensorReading(id: ID!): SensorReading
  }

  type Mutation {
    createSensorReading(input: SensorReadingInput!): SensorReading!
    updateSensorReading(id: ID!, input: SensorReadingInput!): SensorReading!
    deleteSensorReading(id: ID!): Boolean!
  }
`;
