using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.ReflectionModel;

namespace System.ComponentModel.Composition.Primitives
{
    internal static class PrimitivesServices
    {
        public static bool IsGeneric(this ComposablePartDefinition part)
        {
            return part.Metadata.GetValue<bool>(CompositionConstants.IsGenericPartMetadataName);
        }

        public static ImportDefinition GetProductImportDefinition(this ImportDefinition import)
        {
            IPartCreatorImportDefinition partCreatorDefinition = import as IPartCreatorImportDefinition;

            if (partCreatorDefinition != null)
            {
                return partCreatorDefinition.ProductImportDefinition;
            }
            else
            {
                return import;
            }
        }

        internal static IEnumerable<string> GetCandidateContractNames(this ImportDefinition import, ComposablePartDefinition part)
        {
            import = import.GetProductImportDefinition();
            string contractName = import.ContractName;
            string genericContractName = import.Metadata.GetValue<string>(CompositionConstants.GenericContractMetadataName);
            int[] importParametersOrder = import.Metadata.GetValue<int[]>(CompositionConstants.GenericImportParametersOrderMetadataName);
            if (importParametersOrder != null)
            {
                int partArity = part.Metadata.GetValue<int>(CompositionConstants.GenericPartArityMetadataName);
                if (partArity > 0)
                {
                    contractName = GenericServices.GetGenericName(contractName, importParametersOrder, partArity);
                }
            }

            yield return contractName;
            if (!string.IsNullOrEmpty(genericContractName))
            {
                yield return genericContractName;
            }
        }


        internal static bool IsImportDependentOnPart(this ImportDefinition import, ComposablePartDefinition part, ExportDefinition export, bool expandGenerics)
        {
            import = import.GetProductImportDefinition();
            if (expandGenerics)
            {
                return part.GetExports(import).Any();
            }
            else
            {
                return TranslateImport(import, part).IsConstraintSatisfiedBy(export);
            }
        }

        private static ImportDefinition TranslateImport(ImportDefinition import, ComposablePartDefinition part)
        {
            ContractBasedImportDefinition contractBasedImport = import as ContractBasedImportDefinition;
            if (contractBasedImport == null)
            {
                return import;
            }

            int[] importParametersOrder = contractBasedImport.Metadata.GetValue<int[]>(CompositionConstants.GenericImportParametersOrderMetadataName);
            if (importParametersOrder == null)
            {
                return import;
            }

            int partArity = part.Metadata.GetValue<int>(CompositionConstants.GenericPartArityMetadataName);
            if (partArity == 0)
            {
                return import;
            }

            string contractName = GenericServices.GetGenericName(contractBasedImport.ContractName, importParametersOrder, partArity);
            string requiredTypeIdentity = GenericServices.GetGenericName(contractBasedImport.RequiredTypeIdentity, importParametersOrder, partArity);
            return new ContractBasedImportDefinition(
                         contractName,
                         requiredTypeIdentity,
                         contractBasedImport.RequiredMetadata,
                         contractBasedImport.Cardinality,
                         contractBasedImport.IsRecomposable,
                         false,
                         contractBasedImport.RequiredCreationPolicy,
                         contractBasedImport.Metadata);
        }
    }
}
