#nullable enable

using DevoidEngine.Core;
using MessagePack;
using System.Buffers;

namespace DevoidEngine.Serialization
{
    internal static class MaterialInstanceSerializer
    {
        private static readonly MessagePackSerializerOptions Options =
            MessagePackSerializerOptions.Standard;


        public static void Serialize(
            ref MessagePackWriter writer,
            MaterialInstance? value)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            // [
            //     BaseMaterial GUID,
            //     Property overrides,
            //     Texture overrides
            // ]

            writer.WriteArrayHeader(3);


            // =========================================================
            // Base Material
            // =========================================================

            MessagePackSerializer.Serialize(
                ref writer,
                value.BaseMaterial.Guid,
                Options);


            // =========================================================
            // Property Overrides
            // =========================================================

            var properties =
                value.GetOverriddenProperties();

            // Since this is IEnumerable, materialize it so we know
            // the array count before writing the MessagePack array.
            var propertyList = properties.ToArray();

            writer.WriteArrayHeader(propertyList.Length);

            foreach (string propertyName in propertyList)
            {
                // [
                //     property name,
                //     raw value bytes
                // ]

                writer.WriteArrayHeader(2);

                writer.Write(propertyName);

                ReadOnlySpan<byte> valueBytes =
                    value.GetRawValue(propertyName);

                writer.Write(valueBytes.ToArray());
            }


            // =========================================================
            // Texture Overrides
            // =========================================================

            var textures =
                value.GetTextureOverrides().ToArray();

            writer.WriteArrayHeader(textures.Length);

            foreach (var pair in textures)
            {
                writer.WriteArrayHeader(2);

                // Texture binding name
                writer.Write(pair.Key);

                // Texture asset
                try
                {
                    MessagePack.MessagePackSerializer.Serialize(
                        ref writer,
                        pair.Value?.Guid ?? Guid.Empty,
                        MessagePack.MessagePackSerializerOptions.Standard);
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        "[Serialization] Failed to serialize texture override " +
                        $"'{pair.Key}': " + e.Message);

                    writer.WriteNil();
                }
            }
        }


        public static MaterialInstance? Deserialize(
            ref MessagePackReader reader)
        {
            if (reader.TryReadNil())
                return null;


            // =========================================================
            // Root array
            // =========================================================

            int fieldCount =
                reader.ReadArrayHeader();

            if (fieldCount < 3)
            {
                throw new InvalidDataException(
                    "Invalid MaterialInstance data. " +
                    $"Expected 3 fields, got {fieldCount}.");
            }


            // =========================================================
            // Base Material
            // =========================================================

            Guid materialGuid =
                MessagePackSerializer.Deserialize<Guid>(
                    ref reader,
                    Options);

            if (materialGuid == Guid.Empty)
            {
                Console.WriteLine(
                    "[Serialization] MaterialInstance has no base material.");

                return null;
            }

            Material? material =
                Engine.Instance.AssetManager.Load<Material?>(
                    materialGuid);

            if (material == null)
            {
                Console.WriteLine(
                    "[Serialization] Failed to load Material: " +
                    materialGuid);

                return null;
            }


            var instance =
                new MaterialInstance(material);


            // =========================================================
            // Property Overrides
            // =========================================================

            int propertyCount =
                reader.ReadArrayHeader();

            for (int i = 0; i < propertyCount; i++)
            {
                int propertyFieldCount =
                    reader.ReadArrayHeader();

                if (propertyFieldCount < 2)
                {
                    Console.WriteLine(
                        "[Serialization] Invalid MaterialInstance " +
                        $"property at index {i}.");

                    for (int j = 0; j < propertyFieldCount; j++)
                        reader.Skip();

                    continue;
                }


                string? propertyName =
                    reader.ReadString();

                if (propertyName == null)
                {
                    reader.Skip();
                    continue;
                }


                var sequence =
                    reader.ReadBytes();

                if (sequence == null)
                {
                    Console.WriteLine(
                        "[Serialization] Material property " +
                        $"'{propertyName}' has no value.");

                    continue;
                }

                byte[] rawValue =
                    sequence.Value.ToArray();


                // -----------------------------------------------------
                // Validate against the current material layout
                // -----------------------------------------------------

                if (!material.TryGetVariable(
                        propertyName,
                        out var variable))
                {
                    Console.WriteLine(
                        "[Serialization] Material property " +
                        $"'{propertyName}' no longer exists.");

                    continue;
                }

                if (rawValue.Length != variable!.Size)
                {
                    Console.WriteLine(
                        "[Serialization] Material property " +
                        $"'{propertyName}' has invalid size. " +
                        $"Expected {variable.Size}, " +
                        $"got {rawValue.Length}.");

                    continue;
                }


                instance.SetRawValue(
                    propertyName,
                    rawValue);
            }


            // =========================================================
            // Texture Overrides
            // =========================================================

            int textureCount =
                reader.ReadArrayHeader();

            for (int i = 0; i < textureCount; i++)
            {
                int textureFieldCount =
                    reader.ReadArrayHeader();

                if (textureFieldCount < 2)
                {
                    Console.WriteLine(
                        "[Serialization] Invalid MaterialInstance " +
                        $"texture at index {i}.");

                    for (int j = 0; j < textureFieldCount; j++)
                        reader.Skip();

                    continue;
                }


                string? textureName = reader.ReadString();

                if (textureName == null)
                {
                    reader.Skip();
                    continue;
                }

                Guid textureGuid;

                try
                {
                    textureGuid =
                        MessagePack.MessagePackSerializer.Deserialize<Guid>(
                            ref reader,
                            MessagePack.MessagePackSerializerOptions.Standard);
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        "[Serialization] Failed to deserialize texture override " +
                        $"'{textureName}': " + e.Message);

                    continue;
                }

                if (!material.HasTextureBinding(textureName))
                {
                    Console.WriteLine(
                        $"[Serialization] Texture binding '{textureName}' " +
                        "no longer exists.");

                    continue;
                }

                Texture? texture =
                    textureGuid == Guid.Empty
                        ? Texture.Default
                        : Engine.Instance.AssetManager.Load<Texture?>(
                            textureGuid);

                if (texture != null)
                {
                    instance.SetTexture(textureName, texture);
                } else
                {
                    Console.WriteLine(
                        "[Serialization] Failed to load texture " +
                        $"{textureGuid} for binding '{textureName}'.");
                }
            }


            // =========================================================
            // Future fields
            // =========================================================

            for (int i = 3; i < fieldCount; i++)
            {
                reader.Skip();
            }


            return instance;
        }
    }
}