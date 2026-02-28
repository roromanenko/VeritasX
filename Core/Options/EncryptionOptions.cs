using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Options;

public class EncryptionOptions
{
	public int DekSizeBytes = 32;
	public int IvSizeBytes = 12;
	public int TagSizeBytes = 16;
}
